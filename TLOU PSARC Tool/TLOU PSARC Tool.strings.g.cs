using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace V_.q_
{
    class Q_
    {        
        public static Q_ Q__ = new Q_();
        public string this[int i_] 
        { 
            get 
            { 
                if (C_ == null)
                {
                    using (var A_ = new MemoryStream(Convert.FromBase64String(@"AAEAAAD/////AQAAAAAAAAARAQAAAAUAAAAGAgAAAARQU0FSBgMAAAATSW52YWxpZCAnUFNBUicgZmlsZQYEAAAAAS0GBQAAAAAGBgAAAAIweAs=")))
                        C_ = (string[])new BinaryFormatter().Deserialize(A_); 
                }
                return C_[i_]; 
            } 
        }
        private string[] C_;
    }
}