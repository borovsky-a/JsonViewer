//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace mop.Configurator.Editors.DataConnection
{
	public interface IDataConnectionUIControl
	{
		void Initialize(IDataConnectionProperties connectionProperties);
		void LoadProperties();
	}
}
