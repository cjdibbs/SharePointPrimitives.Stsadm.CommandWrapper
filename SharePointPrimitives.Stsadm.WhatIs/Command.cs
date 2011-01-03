using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SharePointPrimitives.Stsadm.WhatIs {
    public class Command : BaseCommand {
        Guid? id;
        protected override IEnumerable<CommandArgument> CommandArguments {
            get {
                yield return new CommandArgument() {
                    Name = "guid",
                    ArgumentRequired = true,
                    CommandRequired = true,
                    Help = "SharePoint guid to find infomation about",
                    OnCommand = s => {
                        try { id = new Guid(s); }
                        catch { }
                    }
                };
            }
        }

        protected override int Run(string command) {
            if (id == null)
                Out.WriteLine("guid was not in the correct format");

            //try to create a site
            try {
                SPSite site = new SPSite(id.Value);
                if (site != null) {
                    Out.WriteLine("{0} is an SPSite {1}", id, site.Url);
                    return 0;
                }
            } catch (Exception e) {
                Log.Debug(e.Message, e);
            }

            //etc
            Out.WriteLine("Could not find an object for {0}", id);
            return 1;
        }
    }
}
