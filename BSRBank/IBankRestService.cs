using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace BSRBank
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBankRestService" in both code and config file together.
    [ServiceContract]
    public interface IBankRestService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/BankNumber/", 
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json
            )]
        string BankNumber();

        [OperationContract]
        [WebInvoke(UriTemplate = "/transfer", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        TransferResponseData TransferRequest(TransferRequestData data);
    }

    public class TransferRequestData
    {
        public string source { get; set; }
        public string destination { get; set; }
        public string title { get; set; }
        public int amount { get; set; }
    }

    public class TransferResponseData
    {
        public string message { get; set; }
        public Dictionary<string, string> errors { get; set; }
    }
}
