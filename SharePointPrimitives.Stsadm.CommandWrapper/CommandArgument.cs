using System;

namespace SharePointPrimitives.Stsadm {
    public sealed class CommandArgument {
        public string Name { get; set; }
        public string Help { get; set; }
        public bool CommandRequired { get; set; }
        public bool ArgumentRequired { get; set; }
        public Action<string> OnCommand { get; set; }
    }
}
