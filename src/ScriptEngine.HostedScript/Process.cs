﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript
{
    public class Process
    {
        ScriptingEngine _engine;
        IHostApplication _host;
        LoadedModuleHandle _module;

        internal Process(IHostApplication host, LoadedModuleHandle src, ScriptingEngine runtime)
        {
            _host = host;
            _engine = runtime;
            _module = src;
        }

        public int Start()
        {
            try
            {
                _engine.NewObject(_module);
                return 0;
            }
            catch (ScriptInterruptionException e)
            {
                return e.ExitCode;
            }
            catch (Exception e)
            {
                _host.ShowExceptionInfo(e);
                return 1;
            }
            finally
            {
                _engine.Dispose();
                _engine = null;
            }
        }

    }
}
