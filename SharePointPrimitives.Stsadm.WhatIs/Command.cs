﻿#region BSD license
// Copyright 2010 Chris Dibbs. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
//
//   1. Redistributions of source code must retain the above copyright notice, this list of
//      conditions and the following disclaimer.
//
//   2. Redistributions in binary form must reproduce the above copyright notice, this list
//      of conditions and the following disclaimer in the documentation and/or other materials
//      provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY Chris Dibbs ``AS IS'' AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Chris Dibbs OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are those of the
// authors and should not be interpreted as representing official policies, either expressed
// or implied, of Chris Dibbs.
#endregion

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
