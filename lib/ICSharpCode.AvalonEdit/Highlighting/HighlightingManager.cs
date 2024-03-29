﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCodeX.AvalonEdit.Highlighting
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Diagnostics;
  using System.IO;
  using System.Xml;

  using ICSharpCodeX.AvalonEdit.Highlighting.Themes;
  using ICSharpCodeX.AvalonEdit.Utils;
  using System.Windows.Media;

  /// <summary>
	/// Manages a list of syntax highlighting definitions.
	/// </summary>
	/// <remarks>
	/// All memers on this class (including instance members) are thread-safe.
	/// </remarks>
	public class HighlightingManager : IHighlightingDefinitionReferenceResolver
	{
    #region fields
		readonly object lockObj = new object();

    //SortedDictionary for displaying names of highlightings sorted by name
    //The ObservableCollection is exposed for binding and backed by a sorted list
    SortedList<string, IHighlightingDefinition> sortedHighlightingsByName;
    ObservableCollection<IHighlightingDefinition> listHighlightingsByName;

    SortedDictionary<string, IHighlightingDefinition> highlightingsByExtension;
		List<IHighlightingDefinition> allHighlightings;

    HighlightingThemes mHlThemes;
    #endregion fields

    #region constructor
    /// <summary>
    /// Constructor
    /// </summary>
    public HighlightingManager()
    {
      sortedHighlightingsByName = new SortedList<string, IHighlightingDefinition>();
      listHighlightingsByName = new ObservableCollection<IHighlightingDefinition>();

      highlightingsByExtension = new SortedDictionary<string, IHighlightingDefinition>(StringComparer.OrdinalIgnoreCase);
      allHighlightings = new List<IHighlightingDefinition>();

      this.mHlThemes = null;
      this.BackupDynResources = null;
    }
    #endregion constructor;

    #region properties
    /// <summary>
    /// Gets the default HighlightingManager instance.
    /// The default HighlightingManager comes with built-in highlightings.
    /// </summary>
    public static HighlightingManager Instance
    {
      get
      {
        return DefaultHighlightingManager.Instance;
      }
    }

    /// <summary>
    /// Gets a copy of all highlightings.
    /// </summary>
    public ReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions
    {
      get
      {
        lock (lockObj)
        {
          return Array.AsReadOnly(allHighlightings.ToArray());
        }
      }
    }

    /// <summary>
    /// Gets a - sorted by name - list of all highlightings. This property uses an observable
    /// collection to support WPF collection bindings with best possible performance.
    /// </summary>
    public ObservableCollection<IHighlightingDefinition> HighlightingDefinitionsSortedByName
    {
      get
      {
        lock (lockObj)
        {
          return this.listHighlightingsByName;
        }
      }
    }

    /// <summary>
    /// Gets the names of the registered highlightings.
    /// </summary>
    [ObsoleteAttribute("Use the HighlightingDefinitions property instead.")]
    public IEnumerable<string> HighlightingNames
    {
      get
      {
        lock (lockObj)
        {
          return new List<string>(this.sortedHighlightingsByName.Keys);
        }
      }
    }

    /// <summary>
    /// Get property to style standard highlighting definitions with
    /// custom colors (foreground, background) and styles (italic, bold)
    /// </summary>
    public HighlightingThemes HlThemes
    {
      get
      {
        lock (lockObj)
        {
          return this.mHlThemes;
        }
      }
      
      protected set
      {
        lock (lockObj)
        {
          if (this.mHlThemes != value)
          {
            this.mHlThemes = value;
          }
        }
      }
    }

    /// <summary>
    /// Get/set property to undo customization changes applied to <seealso cref="SolidColorBrush"/>
    /// dynamic resources (these resources need to be reset to null on next chnage of theme or they
    /// will not be set correctly when the user switches themes twice).
    /// 
    /// This theme customization is the base for custom WPF theming of the editor environment
    /// (background color, hyperlink color etc. controled by XSHD file based settings).
    /// </summary>
    public List<string> BackupDynResources { get; set; }
    #endregion properties

    #region methods
    /// <summary>
		/// Gets a highlighting definition by name.
		/// Returns null if the definition is not found.
		/// </summary>
    /// <param name="nameofHighlightingDefinition">Name of highlighting definition to get</param>
		public IHighlightingDefinition GetDefinition(string nameofHighlightingDefinition)
		{
			lock (lockObj)
      {
        //Dirkster99 Bugfix
        if (sortedHighlightingsByName == null)
          return null;

        //Dirkster99 Bugfix
        if (nameofHighlightingDefinition == null)
          throw new ArgumentException("Name of Highlighting Definition parameter cannot be null.");

        IHighlightingDefinition rh;
        if (sortedHighlightingsByName.TryGetValue(nameofHighlightingDefinition, out rh))
				  return rh;
			  else
				  return null;
			}
		}

    /// <summary>
    /// Dirkster99
    /// Initialize the highlighting definitions to force re-load of definitions
    /// </summary>
    public void InitializeDefinitions(HighlightingThemes hlThemes = null )
    {
      lock (lockObj)
      {
        this.sortedHighlightingsByName.Clear();

        this.listHighlightingsByName.Clear();
        this.highlightingsByExtension.Clear();
        this.allHighlightings.Clear();

        this.mHlThemes = hlThemes;
      }
    }
		  
    /// <summary>
		/// Gets a highlighting definition by extension.
		/// Returns null if the definition is not found.
		/// </summary>
		public IHighlightingDefinition GetDefinitionByExtension(string extension)
		{
			lock (lockObj) {
				IHighlightingDefinition rh;
				if (highlightingsByExtension.TryGetValue(extension, out rh))
					return rh;
				else
					return null;
			}
		}
		
		/// <summary>
		/// Registers a highlighting definition.
		/// </summary>
		/// <param name="name">The name to register the definition with.</param>
		/// <param name="extensions">The file extensions to register the definition for.</param>
		/// <param name="highlighting">The highlighting definition.</param>
		public void RegisterHighlighting(string name, string[] extensions, IHighlightingDefinition highlighting)
		{
			if (highlighting == null)
				throw new ArgumentNullException("highlighting");
			
			lock (lockObj)
      {
        ////HighlightingThemes.PrintThemeToHTML(highlighting, this.mHlThemes);  // Debug/Document highlighting themes with HTML output

        HighlightingThemes.ApplyHighlightingTheme(highlighting, this.mHlThemes);

        this.allHighlightings.Add(highlighting);

				if (name != null)
        {
					this.sortedHighlightingsByName[name] = highlighting;
          this.listHighlightingsByName.Insert(this.sortedHighlightingsByName.IndexOfKey(name), highlighting);
				}

				if (extensions != null)
        {
					foreach (string ext in extensions)
          {
            this.highlightingsByExtension[ext] = highlighting;
					}
				}
			}
		}

		/// <summary>
		/// Registers a highlighting definition.
		/// </summary>
		/// <param name="name">The name to register the definition with.</param>
		/// <param name="extensions">The file extensions to register the definition for.</param>
		/// <param name="lazyLoadedHighlighting">A function that loads the highlighting definition.</param>
		public void RegisterHighlighting(string name, string[] extensions, Func<IHighlightingDefinition> lazyLoadedHighlighting)
		{
			if (lazyLoadedHighlighting == null)
				throw new ArgumentNullException("lazyLoadedHighlighting");

			RegisterHighlighting(name, extensions, new DelayLoadedHighlightingDefinition(name, lazyLoadedHighlighting));
		}
		#endregion methods

    #region internal classes
    internal sealed class DelayLoadedHighlightingDefinition : IHighlightingDefinition
    {
      readonly object lockObj = new object();
      readonly string name;
      Func<IHighlightingDefinition> lazyLoadingFunction;
      IHighlightingDefinition definition;
      Exception storedException;

      public DelayLoadedHighlightingDefinition(string name, Func<IHighlightingDefinition> lazyLoadingFunction)
      {
        this.name = name;
        this.lazyLoadingFunction = lazyLoadingFunction;
      }

      public string Name
      {
        get
        {
          if (name != null)
            return name;
          else
            return GetDefinition().Name;
        }
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
                                                       Justification = "The exception will be rethrown")]
      IHighlightingDefinition GetDefinition()
      {
        Func<IHighlightingDefinition> func;
        lock (lockObj)
        {
          if (this.definition != null)
            return this.definition;
          func = this.lazyLoadingFunction;
        }
        Exception exception = null;
        IHighlightingDefinition def = null;
        try
        {
          using (var busyLock = BusyManager.Enter(this))
          {
            if (!busyLock.Success)
              throw new InvalidOperationException("Tried to create delay-loaded highlighting definition recursively. Make sure the are no cyclic references between the highlighting definitions.");
            def = func();
          }
          if (def == null)
            throw new InvalidOperationException("Function for delay-loading highlighting definition returned null");
        }
        catch (Exception ex)
        {
          exception = ex;
        }
        lock (lockObj)
        {
          this.lazyLoadingFunction = null;
          if (this.definition == null && this.storedException == null)
          {
            this.definition = def;
            this.storedException = exception;
          }
          if (this.storedException != null)
            throw new HighlightingDefinitionInvalidException("Error delay-loading highlighting definition", this.storedException);
          return this.definition;
        }
      }

      public HighlightingRuleSet MainRuleSet
      {
        get
        {
          return GetDefinition().MainRuleSet;
        }
      }

      public HighlightingRuleSet GetNamedRuleSet(string name)
      {
        return GetDefinition().GetNamedRuleSet(name);
      }

      public HighlightingColor GetNamedColor(string name)
      {
        return GetDefinition().GetNamedColor(name);
      }

      public IEnumerable<HighlightingColor> NamedHighlightingColors
      {
        get
        {
          return GetDefinition().NamedHighlightingColors;
        }
      }

      public override string ToString()
      {
        return this.Name;
      }
    }

    internal sealed class DefaultHighlightingManager : HighlightingManager
    {
      public new static readonly DefaultHighlightingManager Instance = new DefaultHighlightingManager();

      public DefaultHighlightingManager()
      {
        Resources.RegisterBuiltInHighlightings(this);
      }

      // Registering a built-in highlighting
      internal void RegisterHighlighting(string name, string[] extensions, string resourceName)
      {
        try
        {
#if DEBUG
          // don't use lazy-loading in debug builds, show errors immediately
          Xshd.XshdSyntaxDefinition xshd;
          using (Stream s = Resources.OpenStream(resourceName))
          {
            using (XmlTextReader reader = new XmlTextReader(s))
            {
              xshd = Xshd.HighlightingLoader.LoadXshd(reader, false);
            }
          }
          Debug.Assert(name == xshd.Name);
          if (extensions != null)
            Debug.Assert(System.Linq.Enumerable.SequenceEqual(extensions, xshd.Extensions));
          else
            Debug.Assert(xshd.Extensions.Count == 0);

          // round-trip xshd:
          //					string resourceFileName = Path.Combine(Path.GetTempPath(), resourceName);
          //					using (XmlTextWriter writer = new XmlTextWriter(resourceFileName, System.Text.Encoding.UTF8)) {
          //						writer.Formatting = Formatting.Indented;
          //						new Xshd.SaveXshdVisitor(writer).WriteDefinition(xshd);
          //					}
          //					using (FileStream fs = File.Create(resourceFileName + ".bin")) {
          //						new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(fs, xshd);
          //					}
          //					using (FileStream fs = File.Create(resourceFileName + ".compiled")) {
          //						new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(fs, Xshd.HighlightingLoader.Load(xshd, this));
          //					}

          RegisterHighlighting(name, extensions, Xshd.HighlightingLoader.Load(xshd, this));
#else
					RegisterHighlighting(name, extensions, LoadHighlighting(resourceName));
#endif
        }
        catch (HighlightingDefinitionInvalidException ex)
        {
          throw new InvalidOperationException("The built-in highlighting '" + name + "' is invalid.", ex);
        }
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                                                       Justification = "LoadHighlighting is used only in release builds")]
      Func<IHighlightingDefinition> LoadHighlighting(string resourceName)
      {
        Func<IHighlightingDefinition> func = delegate
        {
          Xshd.XshdSyntaxDefinition xshd;
          using (Stream s = Resources.OpenStream(resourceName))
          {
            using (XmlTextReader reader = new XmlTextReader(s))
            {
              // in release builds, skip validating the built-in highlightings
              xshd = Xshd.HighlightingLoader.LoadXshd(reader, true);
            }
          }
          return Xshd.HighlightingLoader.Load(xshd, this);
        };
        return func;
      }
    }
    #endregion
	}
}
