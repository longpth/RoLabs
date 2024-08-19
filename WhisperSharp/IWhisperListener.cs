using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperSharp
{
    public interface IWhisperListener
    {
        public void OnUpdateReceived(String message);
        public void OnResultReceived(String result);
    }
}
