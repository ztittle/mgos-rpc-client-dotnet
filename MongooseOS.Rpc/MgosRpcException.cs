using System;

namespace MongooseOS.Rpc
{
    public class MgosRpcException : Exception
    {
        public MgosRpcException(MgosRpcError error)
            : base(error.Message)
        {
            Error = error;
        }

        public MgosRpcError Error { get; }
    }
}
