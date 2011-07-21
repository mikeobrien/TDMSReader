using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TDMSReader
{
    public class File : IEnumerable<Segment>
    {
        private string _path;

        public File(string path)
        {
            _path = path;
        }

        public IEnumerator<Segment> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
