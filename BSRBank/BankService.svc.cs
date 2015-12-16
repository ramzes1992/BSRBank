using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;

namespace BSRBank
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class BankService : IBankService
    {
        const string localBankNumber = "00106068";
        private Random _rand = new Random();
        private Entities _db = new Entities();

        public string GetBankNumber()
        {
            return localBankNumber;
        }

        public List<AccountEntry> GetAccountsNumbers(string token)
        {
            if (IsTokenValid(token))
            {
                var client = GetClientByToken(token);
                return _db.Accounts.Where(a => a.ClientId.Equals(client.Id)).Select(a => new AccountEntry()
                {
                    AccountNumber = a.AccountNumber,
                    Amount = a.Amount,
                }).ToList();
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
            if (IsTokenValid(token))
            {
                var clientId = GetClientByToken(token).Id;
                var clientAccount = _db.Accounts
                    .SingleOrDefault(a => a.ClientId.Equals(clientId)
                    && a.AccountNumber.Equals(accountNumber));

                if (clientAccount != null)
                {
                    return _db.Operations
                        .Where(o => o.Destination.Equals(accountNumber)
                                    || o.Source.Equals(accountNumber)).Select(o => new OperationEntry()
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

        public string CreateNewAccount(string token)
        {
            if (IsTokenValid(token))
            {
                var client = GetClientByToken(token);
                if (client != null)
                {
                    var newAccountNumber = GenerateAccountNumber();

                    _db.Accounts.Add(new Account()
                    {
                        AccountNumber = newAccountNumber,
                        Amount = 0,
                        ClientId = client.Id,
                    });
                    _db.SaveChanges();
                    return newAccountNumber;
                }
                else
                {
                    throw new FaultException("Client does not exist");
                }
            }

            throw new FaultException("Invalid Token");
        }

        public string TransferRequest(OperationEntry operation, string token)
        {
            if (IsTokenValid(token))
            {
                Client client = GetClientByToken(token);
                Account account = _db.Accounts.SingleOrDefault(a => a.ClientId.Equals(client.Id) && a.AccountNumber.Equals(operation.Source));
                if (account != null)
                {
                    if (operation.Destination.Equals(operation.Source))
                    {
                        throw new FaultException("Invalid Destination Account Number");
                    }

                    if (operation.Amount <= 0 || account.Amount < operation.Amount)
                    {
                        throw new FaultException("Invalid amount");
                    }

                    if (operation.Source.Substring(0, 2) != WyliczNRB(operation.Source.Substring(2)))
                    {//suma się nie zgadza
                        throw new FaultException("Invalid Source Check sum");
                    }

                    if (operation.Destination.Substring(0, 2) != WyliczNRB(operation.Destination.Substring(2)))
                    {//suma się nie zgadza
                        throw new FaultException("Invalid Destination Check sum");
                    }

                    if (operation.Destination.Substring(2, 8).Equals(GetBankNumber()))
                    {// same bank

                        TransferToLocalAccount(operation);

                        return "OK";
                    }
                    else
                    {// other bank

                        //if(bank is on the list) TOOD: send request to other bank
                        //else throw Exception;

                        throw new FaultException("Unrecognize Destination Account Numebr");
                    }
                }
                else
                {
                    throw new FaultException("Invalid Account Number");
                }
            }
            else
            {
                throw new FaultException("Invalid Token");
            }

        }

        private void TransferToLocalAccount(OperationEntry operation)
        {
            Operation op = new Operation()
            {
                Amount = operation.Amount,
                Date = DateTime.Now,
                Destination = operation.Destination,
                Source = operation.Source,
                Text = operation.Title,
            };

            var sourceAccount = _db.Accounts.Single(a => a.AccountNumber.Equals(operation.Source));
            var destinationAccount = _db.Accounts.Single(a => a.AccountNumber.Equals(operation.Destination));

            sourceAccount.Amount -= operation.Amount;
            destinationAccount.Amount += operation.Amount;

            _db.Operations.Add(op);

            _db.SaveChanges();
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

        private string WyliczNRB(string bban)
        {
            if (string.IsNullOrEmpty(bban))
                throw new ArgumentException("Nie podano numeru rachunku.");
            bban = bban.Replace(" ", null); // usunięcie ewentualnych spacji
            if (!Regex.IsMatch(bban, @"^\d{24}$"))
                throw new ArgumentException("Podany numer rachunku jest nieprawidłowy.");

            string nr2 = bban + "252100"; // A=10, B=11, ..., L=21, ..., P=25 oraz 2 zera
            int modulo = 0;
            foreach (char znak in nr2)
                modulo = (10 * modulo + int.Parse(znak.ToString())) % 97;
            modulo = 98 - modulo;

            // zwrócenie w postaci czytelnej dla człowieka
            return string.Format("{0:00}", modulo);
        }

        private string GenerateAccountNumber()
        {
            string result = "";

            do
            {
                var seed = _rand.Next(1, int.MaxValue).ToString();
                result = GetBankNumber() + new string('0', 16 - seed.Length) + seed;
            } while (_db.Accounts.Any(a => a.AccountNumber.Equals(result)));

            result = WyliczNRB(result) + result;

            return result;
        }
    }
}
