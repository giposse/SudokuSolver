﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SudokuSolver {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SudokuSolver.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to column.
        /// </summary>
        internal static string Column {
            get {
                return ResourceManager.GetString("Column", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Game Definition.
        /// </summary>
        internal static string GameDefinition {
            get {
                return ResourceManager.GetString("GameDefinition", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to group.
        /// </summary>
        internal static string Group {
            get {
                return ResourceManager.GetString("Group", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} of {0} cells rule found in {1}.
        /// </summary>
        internal static string NofNCommentTemplate {
            get {
                return ResourceManager.GetString("NofNCommentTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The only possible value left for cell [{0}, {1}] is {2}..
        /// </summary>
        internal static string OnlyPossibleValueTemplate {
            get {
                return ResourceManager.GetString("OnlyPossibleValueTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to row.
        /// </summary>
        internal static string Row {
            get {
                return ResourceManager.GetString("Row", resourceCulture);
            }
        }
    }
}