using System.Collections.Generic;
using System.Xml.Serialization;

namespace UpdateBuilder.Models
{
	[XmlRoot("File")]
	public class FileModel
	{
		public string Name
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

		[XmlIgnore]
		public string FullPath
		{
			get;
			set;
		}

		public string Path
		{
			get;
			set;
		}

		public long Size
		{
			get;
			set;
		}

		public string Hash
		{
			get;
			set;
		}

		public bool QuickUpdate
		{
			get;
			set;
		} = true;


		public bool CheckHash
		{
			get;
			set;
		} = true;


		[XmlArray("FileUpdates")]
		public List<FileUpdateModel> FileUpdates
		{
			get;
			set;
		} = new List<FileUpdateModel>();

	}
}
