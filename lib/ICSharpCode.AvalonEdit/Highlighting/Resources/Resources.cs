﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.IO;

namespace ICSharpCodeX.AvalonEdit.Highlighting
{
	static class Resources
	{
		static readonly string Prefix = typeof(Resources).FullName + ".";
		
		public static Stream OpenStream(string name)
		{
			Stream s = typeof(Resources).Assembly.GetManifestResourceStream(Prefix + name);
			if (s == null)
				throw new FileNotFoundException("The resource file '" + name + "' was not found.");
			return s;
		}
		
		internal static void RegisterBuiltInHighlightings(HighlightingManager.DefaultHighlightingManager hlm)
		{
      /*** Dirkster99 BugFix - disabled this function to load xshd's from Edi's sub-folder only.
			hlm.RegisterHighlighting("XmlDoc", null, "XmlDoc.xshd");

			hlm.RegisterHighlighting("C#", new[] { ".cs" }, "CSharp-Mode.xshd");
			
			hlm.RegisterHighlighting("JavaScript", new[] { ".js" }, "JavaScript-Mode.xshd");

			hlm.RegisterHighlighting("HTML", new[] { ".htm", ".html" }, "HTML-Mode.xshd");
			hlm.RegisterHighlighting("ASP/XHTML", new[] { ".asp", ".aspx", ".asax", ".asmx", ".ascx", ".master" }, "ASPX.xshd");

      hlm.RegisterHighlighting("Boo", new[] { ".boo" }, "Boo.xshd");
      hlm.RegisterHighlighting("Coco", new[] { ".atg" }, "Coco-Mode.xshd");
      hlm.RegisterHighlighting("CSS", new[] { ".css" }, "CSS-Mode.xshd");
      hlm.RegisterHighlighting("C++", new[] { ".c", ".h", ".cc", ".cpp" , ".hpp" }, "CPP-Mode.xshd");
      hlm.RegisterHighlighting("Java", new[] { ".java" }, "Java-Mode.xshd");
      hlm.RegisterHighlighting("Patch", new[] { ".patch", ".diff" }, "Patch-Mode.xshd");
      hlm.RegisterHighlighting("PowerShell", new[] { ".ps1", ".psm1", ".psd1" }, "PowerShell.xshd");
      hlm.RegisterHighlighting("PHP", new[] { ".php" }, "PHP-Mode.xshd");
      hlm.RegisterHighlighting("TeX", new[] { ".tex" }, "Tex-Mode.xshd");
      hlm.RegisterHighlighting("VBNET", new[] { ".vb" }, "VBNET-Mode.xshd");

      // Dirkster99 BugFix for extended highlighting of XML based documents
      hlm.RegisterHighlighting("XML", (".aml;.xml;.xsl;.xslt;.xsd;.manifest;.config;.addin;.xshd;.wxs;.wxi;.wxl;.proj;.csproj;.drproj;.vbproj;.ilproj;.booproj;.build;.xfrm;.targets;.xaml;.xpt;.xft;.map;.wsdl;.disco;.ps1xml;.nuspec").Split(';'), "XML-Mode.xshd");
       ***/
    }
	}
}
