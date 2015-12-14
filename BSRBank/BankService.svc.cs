using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;


namespace BSRBank
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class BankService : IBankService
    {
        const string localBankNumber = "00106068";

        private Entities _db = new Entities();

        public string GetBankNumber()
        {
            return localBankNumber;
        }

        public string[] GetAccountsNumbers(string token)
        {
            if (IsTokenValid(token))
            {
                var client = GetClientByToken(token);
                return _db.Accounts.Where(a => a.ClientId.Equals(client.Id)).Select(a => a.AccountNumber).ToArray();
            }

            throw new FaultException("Invalid Token");
        }

        public string LogIn(string username, string password)
        {
            if (_db.Clients.Any(c => c.Username.Equals(username)
                                     && c.Password.Equals(password)))
            {
                Session session = new Session()
                {
                    Date = DateTime.Now,
                    Token = Guid.NewGuid().ToString(),
                    ClientId = _db.Clients.Single(c => c.Username.Equals(username)).Id,
                };
                _db.Sessions.Add(session);
                _db.SaveChanges();

                return session.Token;
            }

            throw new FaultException("Invalid username or password");
        }

        public List<OperationEntry> GetAccountHistory(string accountNumber, string token)
        {
            if(IsTokenValid(token))
            {
                var clientId = _db.Sessions.Single(s => s.Token.Equals(token)).ClientId;
                var clientAccount = _db.Accounts
                    .SingleOrDefault(a => a.ClientId.Equals(clientId)
                    && a.AccountNumber.Equals(accountNumber));

                if(clientAccount != null)
                {
                    return _db.Operations
                        .Where(o => o.Destination.Equals(accountNumber)
                                    && o.Source.Equals(accountNumber)).Select(o => new OperationEntry()
                                    {
                                        Amount = o.Amount,
                                        Source = o.Source,
                                        Destination = o.Destination,
                                        Date = o.Date, 
                                    }).ToList();                   
                }
                else
                {
                    throw new FaultException("Invalid Account Number");
                }
            }

            throw new FaultException("Invalid Token");
        }

        private bool IsTokenValid(string token)
        {
            var time = DateTime.Now.AddMinutes(-30);

            return (_db.Sessions.Any(s => s.Token.Equals(token) && s.Date > time));
        }

        private Client GetClientByToken(string token)
        {
            var clientId = _db.Sessions.Single(s => s.Token.Equals(token)).ClientId;
            return _db.Clients.Single(c => c.Id.Equals(clientId));
        }

    }
}
