﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using DataLayer.Interfaces;

namespace DataLayer.CascadeEfClasses
{
    public class Company : ICascadeSoftDelete
    {
        public int Id { get; set; }

        public string CompanyName { get; set; }

        public HashSet<Quote> Quotes { get; set; }
        public byte SoftDeleteLevel { get; set; }
    }
}