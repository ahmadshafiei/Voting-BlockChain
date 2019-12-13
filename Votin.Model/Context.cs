using System;
using System.Collections.Generic;
using System.Text;
using Votin.Model.Entities;

namespace Votin.Model
{
    public class Context
    {
        public static List<Election> Elections { get; set; } = new List<Election>();
    }
}
