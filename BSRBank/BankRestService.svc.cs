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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "BankRestService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select BankRestService.svc or BankRestService.svc.cs at the Solution Explorer and start debugging.
    public class BankRestService : IBankRestService
    {
        const string localBankNumber = "00106068";
        private Random _rand = new Random();
        private Entities _db = new Entities();

        public string BankNumber()
        {
            return localBankNumber;
        }

        public TransferResponseData TransferRequest(TransferRequestData data)
        {
            var context = WebOperationContext.Current;

            var result = new TransferResponseData()
            {
                message = "OK",
                errors = new Dictionary<string, string>(),
            };


            Account account = _db.Accounts.SingleOrDefault(a => a.AccountNumber.Equals(data.destination));
            if (account != null)
            {
                if (data.destination.Equals(data.source))
                {
                    context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    result.message = "INVALID";
                    result.errors["destination"] = "Invalid Destination Number";
                    return result;
                }

                if (data.amount <= 0 || account.Amount < data.amount)
                {
                    context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    result.message = "INVALID";
                    result.errors["amount"] = "Invalid amount" ;
                    return result;
                }

                if (data.source.Substring(0, 2) != WyliczNRB(data.source.Substring(2)))
                {//suma się nie zgadza
                    context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    result.message = "INVALID";
                    result.errors["source"] = "Invalid Source Check sum" ;
                    return result;
                }

                if (data.destination.Substring(0, 2) != WyliczNRB(data.destination.Substring(2)))
                {//suma się nie zgadza
                    context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    result.message = "INVALID";
                    result.errors["destination"] = "Invalid Destination Check sum";
                    return result;
                }

                if (data.destination.Substring(2, 8).Equals(localBankNumber))
                {// same bank
                 // przelew odrazu

                    TransferToLocalAccount(new OperationEntry()
                    {
                        Amount = data.amount,
                        Destination =data.destination,
                        Source = data.source,
                        Title = data.title,
                    });

                    context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
                    result.message = "OK";
                    result.errors = null;
                    return result;
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
                context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                result.message = "INVALID";
                result.errors["destination"] = "Invalid Destination Number";
                return result;
            }

            return result;

        }

        private bool IsTokenValid(string token)
        {
            var time = DateTime.Now.AddMinutes(-30);

            return (_db.Sessions.Any(s => s.Token.Equals(token) && s.Date > time));
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

    }
}
