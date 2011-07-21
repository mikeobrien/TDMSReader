using System.Collections;
using System.Collections.Generic;

namespace TDMSReader
{
    public class SegmentEnumerator : IEnumerator<Segment>
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        public Segment Current
        {
            get { throw new System.NotImplementedException(); }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}