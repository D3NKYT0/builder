using System.Collections.Generic;
using System.Xml.Serialization;

namespace UpdateBuilder.Models
{
	[XmlRoot("Folder")]
	public class FolderModel
	{
		public string Name
		{
			get;
			set;
		}

		[XmlIgnore]
		public string Path
		{
			get;
			set;
		}

		[XmlIgnore]
		public ModifyType ModifyType
		{
			get;
			set;
		}

		[XmlArray("Folders")]
		public List<FolderModel> Folders
		{
			get;
			set;
		} = new List<FolderModel>();


		[XmlArray("Files")]
		public List<FileModel> Files
		{
			get;
			set;
		} = new List<FileModel>();

	}
}
