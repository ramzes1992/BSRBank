using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace BSRBank
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IBankService
    {

        [OperationContract]
        string GetBankNumber();

        [OperationContract]
        string LogIn(string username, string password);

        [OperationContract]
        List<AccountEntry> GetAccountsNumbers(string token); 

        [OperationContract]
        List<OperationEntry> GetAccountHistory(string accountNumber, string token);

        [OperationContract]
        string TransferRequest(OperationEntry operation, string token);

        [OperationContract]
        string CreateNewAccount(string token);

    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class OperationEntry
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string Destination { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public int Amount { get; set; }
    }

    [DataContract]
    public class AccountEntry
    {
        [DataMember]
        public string AccountNumber { get; set; }

        [DataMember]
        public int Amount { get; set; }
    }
}
