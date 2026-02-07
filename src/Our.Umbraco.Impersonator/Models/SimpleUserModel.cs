using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Extensions;

namespace Our.Umbraco.Impersonator.Models
{
    public class SimpleUserModel
    {
        public Guid Key { get; set; }
        public string Name { get; set; }
    }
}
