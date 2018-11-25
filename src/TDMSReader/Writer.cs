using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NationalInstruments.Tdms
{
    public class Writer
    {
        private readonly BinaryWriter _writer;

        public Stream BaseStream { get { return _writer.BaseStream; } }

        public Writer(Stream stream)
        {
            _writer = new BinaryWriter(stream);
        }

        public void WriteSegment(long offset, Reader.Segment leadin)
        {
            _writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(Encoding.ASCII.GetBytes(leadin.Identifier), 0, 4);
            Int32 tableOfContentsMask = 0;
            if (leadin.TableOfContents.ContainsNewObjects) tableOfContentsMask |= 1 << 2;
            if (leadin.TableOfContents.HasDaqMxData) tableOfContentsMask |= 1 << 7;
            if (leadin.TableOfContents.HasMetaData) tableOfContentsMask |= 1 << 1;
            if (leadin.TableOfContents.HasRawData) tableOfContentsMask |= 1 << 3;
            if (leadin.TableOfContents.NumbersAreBigEndian) tableOfContentsMask |= 1 << 6;
            if (leadin.TableOfContents.RawDataIsInterleaved) tableOfContentsMask |= 1 << 5;
            _writer.Write(tableOfContentsMask);
            _writer.Write(leadin.Version);
            _writer.Write((Int64)leadin.NextSegmentOffset);
            _writer.Write((Int64)leadin.RawDataOffset);
        }

        public static string GetEncodePath(params string[] path)
        {
            return path == null || path.Length == 0 ? "/" : path.Length == 1 ? "/'" + path[0] + "'" : "/'" + path[0] + "'/'" + path[1] + "'";
        }

        public void WriteMetadata(IList<Reader.Metadata> metadatas)
        {
            _writer.Write((Int32)metadatas.Count);
            foreach(var metadata in metadatas)
            {
                _writer.Write(DataType.String, GetEncodePath(metadata.Path));

                //DAQmx Format Changing scaler 0x1269, DAQmx Digital Line scalar 0x1369
                /* If the raw data index of this object in this segment exactly matches the index the same object had in the previous segment, an unsigned 32-bit integer (0x0000000) will be stored instead of the index information. */
                if (metadata.RawData == null) _writer.Write((Int32)(-1)); 
                else
                {
                    _writer.Write((Int32)20 + (metadata.RawData.DataType == DataType.String ? 8 : 0));
                    _writer.Write((Int32)metadata.RawData.DataType);
                    _writer.Write((Int32)metadata.RawData.Dimension);   //must be 1 
                    _writer.Write((UInt64)metadata.RawData.Count);
                    if (metadata.RawData.DataType == DataType.String) _writer.Write((UInt64)metadata.RawData.Size);
                }
                _writer.Write((Int32)(metadata.Properties?.Count ?? 0));
                foreach(KeyValuePair<string, object> entry in (metadata.Properties ?? new Dictionary<string, object>()))
                {
                    _writer.Write(DataType.String, entry.Key);
                    int dataType = DataType.GetDataType(entry.Value);
                    _writer.Write((Int32)dataType);
                    _writer.Write((Int32)dataType, entry.Value);
                }
            }
        }

        public void WriteRawData(Reader.RawData rawData, IEnumerable<object> data)
        {
            if (rawData.IsInterleaved) WriteRawInterleaved(rawData.DataType, rawData.Size, rawData.InterleaveStride, data);
            else if (rawData.DataType == DataType.String) WriteRawStrings(rawData.Count, data);
            else WriteRawFixed(rawData.DataType, data);
        }

        public void WriteRawInterleaved(int dataType, long dataSize, int interleaveStride, IEnumerable<object> data)
        {
            long offset = _writer.BaseStream.Position;
            foreach (object o in data)
            {
                _writer.Write(dataType, o);
                _writer.BaseStream.Seek(interleaveStride - dataSize, SeekOrigin.Current);
            }
            _writer.BaseStream.Seek(offset + dataSize, SeekOrigin.Begin);   //move cursor to next channel
        }

        public void WriteRawFixed(int dataType, IEnumerable<object> data)
        {
            foreach(object o in data) _writer.Write(dataType, o);
        }

        public void WriteRawStrings(long count, IEnumerable<object> data)
        {
            long indexPosition = _writer.BaseStream.Position;
            long dataOffset = indexPosition + (count * 4);
            long baseOffset = dataOffset;
            foreach (object o in data)
            {
                //write string
                _writer.BaseStream.Seek(dataOffset, SeekOrigin.Begin);
                _writer.Write(Encoding.UTF8.GetBytes((string)o));
                int relative_offset = (int)(_writer.BaseStream.Position - baseOffset);
                int length = (int)(_writer.BaseStream.Position - dataOffset);

                //write index
                _writer.BaseStream.Seek(indexPosition, SeekOrigin.Begin);
                _writer.Write((Int32)relative_offset);

                //increment
                indexPosition += 4;
                dataOffset += length;
            }
            _writer.BaseStream.Seek(dataOffset, SeekOrigin.Begin);  //move cursor to end
        }
    }

    /// <summary>
    /// Small helper class for the writer. This will serialize the binary segment proper.
    /// </summary>
    public class WriteSegment : IDisposable
    {
        private Writer _writer;
        private bool _isOpen = false;
        private long _startOffset;
        private long _rawOffset;
        private bool _ownsStream = false;

        public Stream BaseStream { get { return _writer.BaseStream; } }
        public Reader.Segment Header { get; set; }
        public List<Reader.Metadata> MetaData { get; set; }

        public WriteSegment(Stream stream)
        {
            _writer = new Writer(stream);

            //default header
            Header = new Reader.Segment();
            Header.Identifier = "TDSm";
            Header.Version = 4713;
            Header.TableOfContents = new Reader.TableOfContents();
            Header.TableOfContents.HasMetaData = true;  //it would be rather odd with a TDMS file with no meta data. (Then there would be no raw data either)
            Header.NextSegmentOffset = -1; //no more segments
            Header.RawDataOffset = 0;  //for now

            MetaData = new List<Reader.Metadata>();
        }

        public WriteSegment(string path) : this(new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            _ownsStream = true;
        }

        public Writer Open()
        {
            if (_isOpen) throw new Exception("The TDMS segment is already open. It cannot be re-opened");
            if (MetaData.Count == 0) throw new Exception("First insert some meta data");

            //write header
            _startOffset = BaseStream.Position;
            _writer.WriteSegment(_startOffset, Header);
            _writer.WriteMetadata(MetaData);
            _rawOffset = BaseStream.Position;

            _isOpen = true;
            return _writer;
        }

        public void Close()
        {
            if (!_isOpen) return;

            //Re-write raw and next offset
            long end_offset = BaseStream.Position;
            Header.NextSegmentOffset = end_offset;
            Header.RawDataOffset = _rawOffset - Reader.Segment.Length;
            _writer.WriteSegment(_startOffset, Header);
            _writer.WriteMetadata(MetaData);     //meta data contains byte count. These should be updated before 'closing'
            BaseStream.Seek(end_offset, SeekOrigin.Begin);  //set cursor at end

            //close stream 
            if (_ownsStream) BaseStream.Close();

            _isOpen = false;
        }

        public void Dispose()
        {
            Close();
        }

        #region " Helper functions for creating new TDMS files (with properties) "

        public static Reader.Metadata GenerateStandardProperties(string name, string description, params string[] path)
        {
            Reader.Metadata meta = new Reader.Metadata();
            meta.Path = path;
            meta.Properties = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) meta.Properties.Add("name", name);
            if (!string.IsNullOrEmpty(description)) meta.Properties.Add("description", description);
            return meta;
        }

        public static Reader.Metadata GenerateStandardRoot(string name, string author, string description, DateTime datetime)
        {
            Reader.Metadata meta = GenerateStandardProperties(name, description, new string[0]);
            if (!string.IsNullOrEmpty(author)) meta.Properties.Add("author", author);
            if (datetime != new DateTime(1, 1, 1)) meta.Properties.Add("datetime", datetime);
            return meta;
        }

        public static Reader.Metadata GenerateStandardGroup(string name, string description)
        {
            Reader.Metadata meta = GenerateStandardProperties(name, description, name);
            return meta;
        }

        public static Reader.Metadata GenerateStandardChannel(string groupName, string name, string description, string yUnitString, string xUnitString, string xName, DateTime startTime, double increment, Type dataType, int dataCount, int stringBlobLength = 0)
        {
            Reader.Metadata meta = GenerateStandardProperties(name, description, groupName, name);
            if(!string.IsNullOrEmpty(yUnitString)) meta.Properties.Add("unit_string", yUnitString);
            if(!string.IsNullOrEmpty(xUnitString)) meta.Properties.Add("wf_xunit_string", xUnitString);
            if(!string.IsNullOrEmpty(xName)) meta.Properties.Add("wf_xname", xName);
            if(startTime != new DateTime(1,1,1)) meta.Properties.Add("wf_start_time", startTime);
            if(increment != 0) meta.Properties.Add("wf_increment", increment);

            meta.RawData = new Reader.RawData();
            meta.RawData.DataType = DataType.GetDataType(Activator.CreateInstance(dataType));
            meta.RawData.Count = dataCount;
            meta.RawData.Dimension = 1; //always 1
            if (dataType == typeof(string)) meta.RawData.Size = stringBlobLength;

            return meta;
        }

        #endregion  
    }
}
 