﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OverParse_NT.Client.Properties {
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
    public class Translations {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Translations() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OverParse_NT.Client.Properties.Translations", typeof(Translations).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Browse.
        /// </summary>
        public static string InstallSelectionDialog_Browse {
            get {
                return ResourceManager.GetString("InstallSelectionDialog_Browse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please select your `pso2_bin` install directory.
        /// </summary>
        public static string InstallSelectionDialog_BrowserDescription {
            get {
                return ResourceManager.GetString("InstallSelectionDialog_BrowserDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirm.
        /// </summary>
        public static string InstallSelectionDialog_Confirm {
            get {
                return ResourceManager.GetString("InstallSelectionDialog_Confirm", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Path is not a valid directory.
        /// </summary>
        public static string InstallSelectionDialog_ValidationInvalidDirectory {
            get {
                return ResourceManager.GetString("InstallSelectionDialog_ValidationInvalidDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please select a directory.
        /// </summary>
        public static string InstallSelectionDialog_ValidationNoDirectory {
            get {
                return ResourceManager.GetString("InstallSelectionDialog_ValidationNoDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Directory does not contain `pso2.exe` executable.
        /// </summary>
        public static string InstallSelectionDialog_ValidationNoExecutable {
            get {
                return ResourceManager.GetString("InstallSelectionDialog_ValidationNoExecutable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Valid `pso2__bin` directory selected!.
        /// </summary>
        public static string InstallSelectionDialog_ValidationValid {
            get {
                return ResourceManager.GetString("InstallSelectionDialog_ValidationValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Directory must be named `pso2__bin`.
        /// </summary>
        public static string InstallSelectionDialog_ValidationWrongDirectoryName {
            get {
                return ResourceManager.GetString("InstallSelectionDialog_ValidationWrongDirectoryName", resourceCulture);
            }
        }
    }
}
