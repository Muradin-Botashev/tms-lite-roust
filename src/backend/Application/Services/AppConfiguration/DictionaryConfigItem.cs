using System;
using System.Collections.Generic;
using System.Text;

using DictionaryConfigMethod = System.Func<Domain.Persistables.User,
                                           System.Collections.Generic.List<Domain.Persistables.FieldPropertyItem>,
                                           System.Collections.Generic.List<Domain.Persistables.FieldPropertyItemVisibility>,
                                           Domain.Services.AppConfiguration.UserConfigurationDictionaryItem>;

namespace Application.Services.AppConfiguration
{
    public class DictionaryConfig
    {
        public DictionaryConfigMethod ConfigMethod { get; set; }

        public Type ServiceType { get; set; }
    }
}
