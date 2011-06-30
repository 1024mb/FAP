﻿#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */

#endregion

using Autofac;
using FAP.Application.Controllers;
using FAP.Domain.Verbs;

namespace FAP.Application
{
    public class
        ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConversationController>().As<IConversationController>().SingleInstance();
            builder.RegisterType<PopupWindowController>().As<PopupWindowController>().SingleInstance();
            builder.RegisterType<ConnectionController>().SingleInstance();
            builder.RegisterType<WatchdogController>().SingleInstance();
            builder.RegisterType<InterfaceController>();
            builder.RegisterType<ApplicationCore>().SingleInstance();
        }
    }
}