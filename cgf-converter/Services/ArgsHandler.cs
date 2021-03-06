﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter
{
    public class ArgsHandler
    {
        /// <summary>
        /// Files to process
        /// </summary>
        public List<String> InputFiles { get; internal set; }
        /// <summary>
        /// Location of the Object Files
        /// </summary>
        public DirectoryInfo DataDir { get; internal set; }
        /// <summary>
        /// File to render to
        /// </summary>
        // public String OutputFile { get; internal set; }
        /// <summary>
        /// Directory to render to
        /// </summary>
        public String OutputDir { get; internal set; }
        /// 
        /// <summary>
        /// Allows naming conflicts for mtl file
        /// </summary>
        public Boolean AllowConflicts { get; internal set; }

        /// <summary>
        /// For LODs files.  Adds _out onto the output
        /// </summary>
        public Boolean NoConflicts { get; internal set; }

        /// <summary>
        /// Name to group all meshes under
        /// </summary>
        public Boolean GroupMeshes { get; internal set; }
        /// <summary>
        /// Render CryTek format files
        /// </summary>
        public Boolean Output_CryTek { get; internal set; }
        /// <summary>
        /// Render Wavefront format files
        /// </summary>
        public Boolean Output_Wavefront { get; internal set; }
        /// <summary>
        /// Render Blender format files
        /// </summary>
        public Boolean Output_Blender { get; internal set; }
        /// <summary>
        /// Render COLLADA format files
        /// </summary>
        public Boolean Output_Collada { get; internal set; }
        /// <summary>
        /// Render FBX
        /// </summary>
        public Boolean Output_FBX { get; internal set; }
        /// <summary>
        /// Smooth Faces
        /// </summary>
        public Boolean Smooth { get; internal set; }
        /// <summary>
        /// Flag used to indicate we should convert texture paths to use TIFF instead of DDS
        /// </summary>
        public Boolean TiffTextures { get; internal set; }
        /// <summary>
        /// Flag used to skip the rendering of nodes containing $shield
        /// </summary>
        public Boolean SkipShieldNodes { get; internal set; }
        /// <summary>
        /// Flag used to skip the rendering of nodes containing $proxy
        /// </summary>
        public Boolean SkipProxyNodes { get; internal set; }
        /// <summary>
        /// Flag used to pass exceptions to installed debuggers
        /// </summary>
        public Boolean Throw { get; internal set; }

        public ArgsHandler()
        {
            this.InputFiles = new List<String> { };
        }

        /// <summary>
        /// Take a string, and expand it into a list of files if it is a file filter
        /// 
        /// TODO: Make it understand /**/ format, instead of ONLY supporting FileName wildcards
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private String[] GetFiles(String filter)
        {
            if (File.Exists(filter))
                return new String[] { new FileInfo(filter).FullName };

            String directory = Path.GetDirectoryName(filter);
            if (String.IsNullOrWhiteSpace(directory))
                directory = ".";

            String fileName = Path.GetFileName(filter);
            String extension = Path.GetExtension(filter);

            Boolean flexibleExtension = extension.Contains('*');

            return Directory.GetFiles(directory, fileName, fileName.Contains('?') || fileName.Contains('*') ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(f => flexibleExtension || Path.GetExtension(f).Length == extension.Length)
                .ToArray();
        }

        /// <summary>
        /// Parse command line arguments
        /// </summary>
        /// <param name="inputArgs">list of arguments to parse</param>
        /// <returns>0 on success, 1 if anything went wrong</returns>
        public Int32 ProcessArgs(String[] inputArgs)
        {
            for (int i = 0; i < inputArgs.Length; i++)
            {
                #region Parse Arguments

                switch (inputArgs[i].ToLowerInvariant())
                {
                    #region case "-objectdir" / "-datadir"...

                    // Next item in list will be the Object directory
                    case "-datadir":
                    case "-objectdir":
                        if (++i > inputArgs.Length)
                        {
                            this.PrintUsage();
                            return 1;
                        }

                        this.DataDir = new DirectoryInfo(inputArgs[i].Replace("\"", string.Empty ));

                        Console.WriteLine("Data directory set to {0}", inputArgs[i]);

                        break;

                    #endregion
                    #region case "-out" / "-outdir" / "-outputdir"...

                    // Next item in list will be the output directory
                    case "-out":
                    case "-outdir":
                    case "-outputdir":
                        if (++i > inputArgs.Length)
                        {
                            this.PrintUsage();
                            return 1;
                        }

                        this.OutputDir = new DirectoryInfo(inputArgs[i]).FullName;

                        Console.WriteLine("Output directory set to {0}", inputArgs[i]);

                        break;

                    #endregion
                    #region case "-usage"...

                    case "-usage":
                        this.PrintUsage();
                        return 1;

                    #endregion
                    #region case "-smooth"...

                    case "-smooth":
                        Console.WriteLine("Smoothing Faces");
                        this.Smooth = true;

                        break;

                    #endregion
                    #region case "-blend" / "-blender"...

                    case "-blend":
                    case "-blender":
                        Console.WriteLine("Output format set to Blender (.blend)");
                        this.Output_Blender = true;

                        break;

                    #endregion
                    #region case "-obj" / "-object" / "wavefront"...

                    case "-obj":
                    case "-object":
                    case "-wavefront":
                        Console.WriteLine("Output format set to Wavefront (.obj)");
                        this.Output_Wavefront = true;

                        break;

                    #endregion
                    #region case "-fbx"
                    case "-fbx":
                        Console.WriteLine("Output format set to FBX (.fbx)");
                        this.Output_FBX = true;
                        break;
                    #endregion
                    #region case "-dae" / "-collada"...
                    case "-dae":
                    case "-collada":
                        Console.WriteLine("Output format set to COLLADA (.dae)");
                        this.Output_Collada = true;

                        break;

                    #endregion
                    #region case "-crytek"...
                    case "-cry":
                    case "-crytek":
                        Console.WriteLine("Output format set to CryTek (.cga/.cgf/.chr/.skin)");
                        this.Output_CryTek = true;

                        break;

                    #endregion
                    #region case "-tif" / "-tiff"...

                    case "-tif":
                    case "-tiff":

                        this.TiffTextures = true;

                        break;

                    #endregion
                    #region case "-skipshield" / "-skipshields"...

                    case "-skipshield":
                    case "-skipshields":

                        this.SkipShieldNodes = true;

                        break;

                    #endregion
                    #region case "-skipproxy"...

                    case "-skipproxy":

                        this.SkipProxyNodes = true;

                        break;

                    #endregion
                    #region case "-group"...

                    case "-group":

                        this.GroupMeshes = true;

                        Console.WriteLine("Grouping set to {0}", this.GroupMeshes);

                        break;

                    #endregion
                    #region case "-throw"...

                    case "-throw":
                        Console.WriteLine("Exceptions thrown to debugger");
                        this.Throw = true;

                        break;

                    #endregion
                    #region case "-infile" / "-inputfile"...

                    // Next item in list will be the output filename
                    case "-infile":
                    case "-inputfile":
                        if (++i > inputArgs.Length)
                        {
                            this.PrintUsage();
                            return 1;
                        }

                        this.InputFiles.AddRange(this.GetFiles(inputArgs[i]));

                        Console.WriteLine("Input file set to {0}", inputArgs[i]);

                        break;

                    #endregion
                    #region case "-allowconflict"...
                    case "-allowconflicts":
                    case "-allowconflict":
                        AllowConflicts = true;
                        Console.WriteLine("Allow conflicts for mtl files enabled");
                        break;
                    #endregion
                    #region case "-noconflict"...
                    case "-noconflict":
                    case "-noconflicts":
                        NoConflicts = true;
                        Console.WriteLine("Prevent conflicts for mtl files enabled");
                        break;
                    #endregion


                    #region default...

                    default:
                        this.InputFiles.AddRange(this.GetFiles(inputArgs[i]));

                        Console.WriteLine("Input file set to {0}", inputArgs[i]);

                        break;

                    #endregion
                }

                #endregion
            }

            // Ensure we have a file to process
            if (this.InputFiles.Count == 0)
            {
                this.PrintUsage();
                return 1;
            }

            // Default to Collada (.dae) format
            if (!this.Output_Blender && !this.Output_Collada && !this.Output_Wavefront && !this.Output_FBX)
                this.Output_Collada = true;
            // Default to TIF files (only for Bulkheads crew)
            //this.TiffTextures = true;

            return 0;
        }

        /// <summary>
        /// Print the usage syntax of the executable
        /// </summary>
        public void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("cgf-converter [-usage] | <.cgf file> [-outputfile <output file>] [-objectdir <ObjectDir>] [-obj] [-blend] [-dae] [-smooth] [-throw]");
            Console.WriteLine();
            Console.WriteLine("-usage:           Prints out the usage statement");
            Console.WriteLine();
            Console.WriteLine("<.cgf file>:      Mandatory.  The name of the .cgf, .cga or .skin file to process");
            Console.WriteLine("-outputfile:      The name of the file to write the output.  Default is [root].obj");
            Console.WriteLine("-noconflict:      Use non-conflicting naming scheme (<cgf File>_out.obj)");
            Console.WriteLine("-allowconflict:   Allows conflicts in .mtl file name");
            Console.WriteLine("-objectdir:       The name where the base Objects directory is located.  Used to read mtl file");
            Console.WriteLine("                  Defaults to current directory.");
            Console.WriteLine("-obj:             Export Wavefront format files (Default: true)");
            Console.WriteLine("-blend:           Export Blender format files (Not Implemented)");
            Console.WriteLine("-dae:             Export Collada format files (Partially Implemented)");
            Console.WriteLine("-fbx:             Export FBX format files (Not Implemented)");
            Console.WriteLine("-smooth:          Smooth Faces");
            Console.WriteLine("-group:           Group meshes into single model");
            Console.WriteLine();
            Console.WriteLine("-throw:           Throw Exceptions to installed debugger");
            Console.WriteLine();
        }

        /// <summary>
        /// Print the current arguments of the executable
        /// </summary>
        public void WriteArgs()
        {
            Console.WriteLine();
            Console.WriteLine("*** Submitted args ***");
            // Console.WriteLine("    Input files:            {0}", this.InputFile);
            if (!String.IsNullOrWhiteSpace(this.DataDir.FullName))
            {
                Console.WriteLine("    Object dir:             {0}", this.DataDir);
            }
            if (!String.IsNullOrWhiteSpace(this.OutputDir))
            {
                Console.WriteLine("    Output file:            {0}", this.OutputDir);
            }
            Console.WriteLine("    Smooth Faces:           {0}", this.Smooth);
            Console.WriteLine("    Output to .obj:         {0}", this.Output_Wavefront);
            Console.WriteLine("    Output to .blend:       {0}", this.Output_Blender);
            Console.WriteLine("    Output to .dae:         {0}", this.Output_Collada);
            Console.WriteLine("    Output to .fbx:         {0}", this.Output_FBX);
            Console.WriteLine();
        }

        public bool Verbose { get; set; }
    }
}