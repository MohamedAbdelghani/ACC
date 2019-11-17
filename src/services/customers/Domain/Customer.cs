﻿using ACC.Common.Exceptions;
using ACC.Common.Types;

namespace ACC.Services.Customers.Domain
{
    public class Customer : EntityBase, IIdentifiable
    {
        public string Name { get; protected set; }
        public Address Address { get; protected set; } = new Address();

        public Customer(string id, string name)
            : base(id.ToLower())
        {
            SetName(name);
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new AccException("invalid_customer_name", "Customer name can not be null or empty");
            }
        }

        public void SetAddress(string Line1, string Line2, string city, string state, string country, string postCode)
        {
            // TODO: validate address

            Address.Line1 = Line1;
            Address.Line2 = Line2;
            Address.City = city;
            Address.State = state;
            Address.Country = country;
            Address.PostCode = postCode;

            SetUpdateDate();
        }
    }
}