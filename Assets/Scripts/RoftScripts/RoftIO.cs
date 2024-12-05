using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROFTIOMANAGEMENT
{
    public static class IDHIST
    {
        //Defalt Identity History path.
        private static string IHistPath { get; } = Application.persistentDataPath + "/ihist.ID";

        /// <summary>
        /// Create a new Identity histroy
        /// </summary>
        public static void NewHistory()
        {
            File.Create(IHistPath);
        }

        /// <summary>
        /// Check if Identity History exists.
        /// </summary>
        /// <returns></returns>
        public static bool HistoryExists()
        {
            bool exist = File.Exists(IHistPath);
            return exist;
        }

        /// <summary>
        /// Write a new ID into history file.
        /// </summary>
        /// <param name="_content"></param>
        public static void Write(string _content)
        {
            //We'll reference to the file ihist file.
            //This file will keep track of all the used IDs.
            string iHistPath = Application.persistentDataPath + "/ihist.ID";
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(_content);
            string encodedText = Convert.ToBase64String(bytesToEncode);
            if (File.Exists(iHistPath))
            {
                BinaryWriter binaryWriter = new BinaryWriter(File.Open(iHistPath, FileMode.Append));
                binaryWriter.Write(encodedText + "\n");
                binaryWriter.Flush();
                binaryWriter.Close();
            }
            else
            {
                Debug.Log("ID History File doesn't exist.");
                Application.Quit();
            }
        }

        /// <summary>
        /// Iterate through ihist.ID and return an array of data
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllID()
        {
            //We need something to keep track of all the data that's
            //converted from a binary encode base 64 utf 8 back into a string
            List<string> data = new List<string>();

            //Since we're dealing with a binary file, we have to use a filestream
            //which is mostly suggested, followed by reader with a BinaryReader
            //It'll iterate through the file and decode each value in the file,
            //then adds the decoded string into our data list.
            using (FileStream fileStream = File.OpenRead(IHistPath))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        string curString = binaryReader.ReadString();
                        byte[] decodedBytes = Convert.FromBase64String(curString);
                        string encodedText = Encoding.UTF8.GetString(decodedBytes);
                        data.Add(encodedText);
                    }
                    fileStream.Flush();
                }
                return data.ToArray();
            }
        }

        /// <summary>
        /// Check if the specified GroupID is used in our ID history
        /// </summary>
        /// <param name="_groupID">The GroupId for the method to search for.</param>
        /// <returns>If the GroupID is used.</returns>
        public static bool GROUPIDExists(long _groupID)
        {
            //We'll have to iterate through out ihist.ID file, and parse for GROUPID
            string[] data = GetAllID();

            //We want to ignore the following symbols...
            char[] delimiters = { '-', '(', ')' };

            //We search through our array of data until we've confirm that this
            //GROUPID exists.
            foreach (string id in data)
            {
                if (_groupID == Convert.ToInt64(id.Split(delimiters)[1]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Will a give GroupId, check if there's an existing RoftId.
        /// </summary>
        /// <param name="_roftID">The specified RoftID to search for.</param>
        /// <param name="_groupID">The GroupID that the RoftID is associated with.</param>
        /// <returns>If the RoftID is used.</returns>
        public static bool ROFTIDExistsInGroupID(int _roftID, long _groupID)
        {
            string[] data = GetAllID();

            //We want to ignore the following symbols...
            char[] delimiters = { '-', '(', ')' };

            //Iterate and find matching GroupId first
            foreach (string id in data)
            {
                //We use the Contain method because this will track the specific
                //ID if the GROUPID happens to exists.
                //This prevents any difficulty besides the first to continue a number.

                #region Can't understand? Here's a scenario...
                /*We create a song with it's first difficulty, so it gets 1000000-100.
                 You can keep creating new diffculties from this, incrementing by 1 from 100 (101, 102, etc).

                 But when a new song is made with a new GroupId (so 1000001-100), the first difficulty
                 will be fine, but without the Contain method, the second difficulty of the new song will now be
                 1000001-103 instead of 1000001-101 as it should be.*/
                #endregion

                if (GROUPIDExists(_groupID) && id.Contains(_groupID + "-" + _roftID))
                    return true;
            }
            return false;
        }
    }

    public static class RoftIO
    {
        public static string AsTag(this string tagName)
        {
            return string.Format("[{0}]\n", tagName);
        }

        public static string AsProperty(this string propertyName, object value = null)
        {
            return string.Format("{0}: {1}", propertyName, value);
        }

        public static FileInfo GAME_DIRECTORY { get; private set; } = new FileInfo(Application.persistentDataPath);

        /// <summary>
        /// Find an existing directory.
        /// </summary>
        /// <param name="directory"></param>
        public static bool DirectoryExists(DirectoryInfo _directory) => _directory.Exists;

        /// <summary>
        /// Find an existing directory.
        /// </summary>
        /// <param name="_directoryPath"></param>
        /// <returns></returns>
        public static bool DirectoryExists(string _directoryPath)
        {
            DirectoryInfo directory = new DirectoryInfo(_directoryPath);
            return directory.Exists;
        }

        /// <summary>
        /// If a certain file exists.
        /// </summary>
        /// <param name="_fileName"></param>
        /// <returns></returns>
        public static bool FileExists(FileInfo _fileName) => _fileName.Exists;

        /// <summary>
        /// If a certain file exists.
        /// </summary>
        /// <param name="_filePath"></param>
        /// <returns></returns>
        public static bool FileExists(string _filePath)
        {
            FileInfo _fileName = new FileInfo(_filePath);
            return _fileName.Exists;
        }

        /// <summary>
        /// Create a new RFTM file.
        /// </summary>
        /// <param name="_name">The name of the file.</param>
        /// <param name="_path">The path in which to create the file.</param>
        public static void CreateNewRFTM(string _name, string _path)
        {
            #region Formatting (RFTM FILE FORMAT VERSION 1.06)
            RoftFormat roftFormatObj = RoftFormat.New();
            string[] formatString = roftFormatObj.GetFormatInfo();
            #endregion

            #region Creation/Overwrite Process
            try
            {
                string rftmFilePath = Path.Combine(_path, string.Format("{0}.rftm", _name));

                if (!File.Exists(rftmFilePath))
                {
                    Debug.Log("Creating new .rftm file...");
                    using (StreamWriter rftmWriter = File.CreateText(rftmFilePath))
                    {
                        for (int line = 0; line < formatString.Length; line++)
                            rftmWriter.WriteLine(formatString[line]);
                    }
                    Debug.Log(".rftm file created!");
                }
                else
                {
                    Debug.Log(string.Format("Overwritting {0}.rftm", _name));

                    using (FileStream outStream = File.Open(rftmFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        using (StreamWriter rftmWriter = new StreamWriter(outStream))
                        {
                            rftmWriter.Flush();

                            for (int line = 0; line < formatString.Length; line++)
                                rftmWriter.WriteLine(formatString[line]);
                        }
                    }
                    Debug.Log("Successfully overwritten!");
                }
            }
            catch
            {
                Debug.LogError("Error occurred with Creating/Overwritting .rftm file.");
                return;
            }
            #endregion
        }

        /// <summary>
        /// Generates a ROFTID based on ihist.ID. When a song is first created,
        /// the base value will always be 100. The value above this can be consider a difficulty
        /// of a particular song.
        /// </summary>
        /// <param name="_groupID">The id used to associate with a song.</param>
        /// <returns>A ROFTID, which will be associate with difficulty of a song.</returns>
        public static int GenerateROFTID(long _groupID)
        {
            /*ROFTID is as structured:
             * GROUPID-(100 to 999)
             * Example: 1805121000-841
             */

            int[] numRange =
            {
                100,
                999
            };

            string stringROFTID = "";

            //We will then iterate the ihist file and check if a roftID exists
            //This has to be in a specified GroupID.
            for (int uniqueNum = numRange[0]; uniqueNum < numRange[1] + 1; uniqueNum++)
            {
                if (!IDHIST.ROFTIDExistsInGroupID(uniqueNum, _groupID))
                {
                    stringROFTID += (uniqueNum.ToString());
                    return Convert.ToInt32(stringROFTID);
                }
            }

            return numRange[0];
        }

        /// <summary>
        /// Generate a GROUPID to be able to create a song with its unique ID.
        /// </summary>
        /// <returns>A GROUPID, which is associated with a new song.</returns>
        public static long GenerateGROUPID()
        {
            /*GROUPID is as structured:
             * yy-dd-mm-(1000 to 9990)
             * Example: 1805121000
             * 
             * G - GROUPID
             * 18 - 2018
             * 05 - 5th
             * 12 - Dec
             * Number 1000
             */

            int[] numRange =
            {
            1000,
            9999
        };

            string stringGROUPID = "";

            DateTime date = DateTime.Now;

            string yearDigit = date.ToString("yy");
            string dayDigit = date.ToString("dd");
            string monthDigit = date.ToString("mm");
            string uniqueNum = Random.Range(numRange[0], numRange[1]).ToString();

            stringGROUPID += (yearDigit + monthDigit + dayDigit + uniqueNum);

            return Convert.ToInt64(stringGROUPID);
        }

        /// <summary>
        /// Generate a directory when a new song is created. The directory will always be persistent.
        /// </summary>
        /// <param name="_GROUPID">The songs associated GROUPID</param>
        /// <param name="_songArtist">The Artist of the Song</param>
        /// <param name="_songName">The Name of the Song</param>
        /// <returns>A path of the newly created directory.</returns>
        public static string GenerateDirectory(long? _GROUPID, string _songArtist, string _songName)
        {
            string _path = Application.persistentDataPath + "/Songs/" + "(" + _GROUPID + ") " + _songArtist + "-" + _songName;
            //Create a Directory Specific for a song
            Directory.CreateDirectory(_path);
            return _path;
        }

        /// <summary>
        /// Generate a generate directory. The directory will always be persistent.
        /// </summary>
        /// <param name="_directoryName">The name of that directory.</param>
        /// <returns>A path of the newly created directory.</returns>
        public static string GenerateDirectory(string _directoryName)
        {
            string _path = Application.persistentDataPath + "/" + _directoryName;
            //Create a Directory Specific for a song
            Directory.CreateDirectory(_path);
            return _path;
        }

        /// <summary>
        /// Allows to jump to a tag that is position in a .rftm file.
        /// </summary>
        /// <param name="_tag">The tag to search for.</param>
        /// <param name="m_name">The name of the .rftm file.</param>
        /// <param name="_fileName">The name of the .rftm file.</param>
        /// <param name="_fileDirectory">The path in which to find the .rftm file.</param>
        /// <returns>A position in file.</returns>
        public static int InRFTMJumpTo(string _tag, string _fileDirectory = "")
        {
            //Setting default for directory
            if (_fileDirectory == "")
                _fileDirectory = Application.streamingAssetsPath;


            string line;

            string rftmFilePath = _fileDirectory;


            int targetPosition = 0;
            #region Read .rftm data
            if (File.Exists(rftmFilePath))
            {
                //Read each line, split with a array string
                //Name it separator
                //Then use string.Split(separator, ...)
                char[] separator = { '[', ']' };
                using (StreamReader rftmReader = new StreamReader(rftmFilePath))
                {
                    while (true)
                    {
                        line = rftmReader.ReadLine();

                        if (line == null)
                        {
                            Debug.LogWarning("The tag " + _tag + " doesn't exist.");
                            return -1;
                        }

                        line.Split(separator);
                        if (line.Contains(_tag))
                        {
                            return targetPosition;
                        }
                        targetPosition++;
                    }
                }
            }

            Debug.LogWarning("The specified file doesn't exist: " + _fileDirectory);
            #endregion

            return -1;
        }

        /// <summary>
        /// Read a property value with the targeted tag. Use InRFTMJumpTo method for _startPosition value;
        /// </summary>
        /// <typeparam name="T">The type of property to return.</typeparam>
        /// <param name="_startPosition">The position in file to start looking for the property.</param>
        /// <param name="_property">The property to look for.</param>
        /// <param name="m_name">The name of the .rftm file.</param>
        /// <returns>The value of the specified property</returns>
        public static T ReadPropertyFrom<T>(int _startPosition, string _property, string _path)
        {
            string line;
            string rftmFilePath = _path;

            //This is used to iterate and find the tag specified by the _startPosition parameter.
            int position = 0;

            //Check if it actually found something
            bool foundProperty = false;

            if (File.Exists(rftmFilePath))
            {
                using (StreamReader rftmReader = new StreamReader(rftmFilePath))
                {
                    while (true)
                    {
                        string targetVal = null;

                        line = rftmReader.ReadLine();

                        /*If we iterate through the file and can not find
                         the tag needed to access its property, we
                         return default
                         */
                        if (line == null)
                        {
                            if (!foundProperty) Debug.Log("This tag does not exist");
                            return default;
                        }

                        /*If we're at the position where our tag is defined in file
                         we then check for our property.
                         */
                        if (position > _startPosition)
                        {
                            if (line == "")
                                return default;

                            //If not empty, you can do whatever!!!
                            if (line.Contains(_property))
                            {
                                foundProperty = true;
                                targetVal = line.Replace(_property + ": ", "");

                                return (T)Convert.ChangeType(targetVal, typeof(T));
                            }
                        }
                        position++;
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// Set a property value based on a specified tag name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="_startPosition"></param>
        /// <param name="_property"></param>
        /// <param name="_path"></param>
        /// <returns></returns>
        public static T SetPropertyValue<T>(object value, int _startPosition, string _property, string _path)
        {
            string line;
            string rftmFilePath = _path;

            //This is used to iterate and find the tag specified by the _startPosition parameter.
            int position = 0;

            //Check if it actually found something
            bool foundProperty = false;

            if (File.Exists(rftmFilePath))
            {
                using (StreamWriter rftmWriter = new StreamWriter(rftmFilePath))
                {
                    while (true)
                    {
                        string targetVal = null;

                        line = rftmWriter.ToString();

                        /*If we iterate through the file and can not find
                         the tag needed to access its property, we
                         return default
                         */
                        if (line == null)
                        {
                            if (!foundProperty) Debug.Log("This tag does not exist");
                            return default;
                        }

                        /*If we're at the position where our tag is defined in file
                         we then check for our property.
                         */
                        if (position > _startPosition)
                        {
                            if (line == "")
                                return default;

                            //If not empty, you can do whatever!!!
                            if (line.Contains(_property))
                            {
                                foundProperty = true;
                                rftmWriter.WriteLine("_property".AsProperty(value));
                                targetVal = line.Replace(_property + ": ", "");

                                return (T)Convert.ChangeType(targetVal, typeof(T));
                            }
                        }
                        position++;
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// Get how many objects the song file has
        /// </summary>
        /// <param name="_path"></param>
        /// <returns></returns>
        public static int GetNoteObjectCountInRFTMFile(string _path)
        {
            //Position tracking
            int position = 0;

            //And get the total object
            int totalNotes = 0;

            string rftmFilePath = _path;

            //We won't be returning anything, but we'll see if there's even something writtern
            //at the line, so that we can count.
            string line;

            //Access the Objects tag
            int objectsTag = InRFTMJumpTo("Objects", _path);

            //A bool to check that we've even got to the ObjectsTag
            bool atObjectsTag = false;

            const int FAILURE = -1;
            const int ZERO = 0;


            //We check if the targeted file exists before reading data
            if (File.Exists(rftmFilePath))
            {
                using (StreamReader streamReader = new StreamReader(_path))
                {
                    while (true)
                    {
                        //Read each and every line...
                        line = streamReader.ReadLine();

                        //Untile we come across a line that has no information
                        if (line == null)
                        {
                            //If we have already made it to our targeted tag to read our data
                            //which happens to be [Objects], then we'll teach if 
                            //we got data or not.
                            if (atObjectsTag == true)
                            {
                                if (totalNotes != ZERO)
                                    return totalNotes - 1;
                                else
                                {
                                    Debug.Log("There was no known objects in this file.");
                                    return ZERO;
                                }


                            }
                            else
                            {
                                Debug.Log("This tag doesn't exist.");
                                return FAILURE;
                            }
                        }

                        if (position >= objectsTag)
                        {
                            //We will mark true if we have hit or is beyond the objectsTag
                            //basically reading the tag's properties/objects
                            atObjectsTag = true;

                            totalNotes++;
                        }

                        position++;
                    }
                }
            }
            Debug.Log("Path " + _path + " doesn't exist.");
            return FAILURE;
        }

        public static void AddRecord(string recordString, string _path)
        {
            //Position tracking
            int position = 0;

            //And get the total object
            int totalNotes = 0;

            string rftmFilePath = _path;

            //We won't be returning anything, but we'll see if there's even something writtern
            //at the line, so that we can count.
            string line;

            //Access the Objects tag
            int objectsTag = InRFTMJumpTo("Records", _path);

            //A bool to check that we've even got to the ObjectsTag
            bool atObjectsTag = false;


            //We check if the targeted file exists before reading data
            if (File.Exists(rftmFilePath))
            {
                using (StreamReader streamReader = new StreamReader(_path))
                {
                    while (true)
                    {
                        //Read each and every line...
                        line = streamReader.ReadLine();

                        //Untile we come across a line that has no information
                        if (line == null)
                        {
                            //If we have already made it to our targeted tag to read our data
                            //which happens to be [Objects], then we'll teach if 
                            //we got data or not.
                            if (atObjectsTag == true)
                            {
                                line = recordString;
                            }
                            else
                            {
                                Debug.Log("This tag doesn't exist.");
                                return;
                            }
                        }

                        if (position >= objectsTag)
                        {
                            //We will mark true if we have hit or is beyond the objectsTag
                            //basically reading the tag's properties/objects
                            atObjectsTag = true;

                            totalNotes++;
                        }

                        position++;
                    }
                }
            }
            Debug.Log("Path " + _path + " doesn't exist.");
            return;
        }
    }
}