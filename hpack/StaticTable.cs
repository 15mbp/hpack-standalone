/*
 * Copyright 2014 Twitter, Inc
 * This file is a derivative work modified by 15mbp
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

/*
	Add, Remove, Clear methods
	Preserve compatibility with HTTP/2
*/

namespace hpack
{
	/// <summary>
	/// The StaticTable class.
	/// </summary>
	public static class StaticTable
	{
		/// <summary>
		/// The static table
		/// Appendix A: Static Table
		/// </summary>
		/// <note type="rfc">http://tools.ietf.org/html/rfc7541#appendix-A</note>
		private static List<HeaderField> STATIC_TABLE = [
			/*  1 */new HeaderField(":authority", string.Empty),
			/*  2 */new HeaderField(":method", "GET"),
			/*  3 */new HeaderField(":method", "POST"),
			/*  4 */new HeaderField(":path", "/"),
			/*  5 */new HeaderField(":path", "/index.html"),
			/*  6 */new HeaderField(":scheme", "http"),
			/*  7 */new HeaderField(":scheme", "https"),
			/*  8 */new HeaderField(":status", "200"),
			/*  9 */new HeaderField(":status", "204"),
			/* 10 */new HeaderField(":status", "206"),
			/* 11 */new HeaderField(":status", "304"),
			/* 12 */new HeaderField(":status", "400"),
			/* 13 */new HeaderField(":status", "404"),
			/* 14 */new HeaderField(":status", "500"),
			/* 15 */new HeaderField("accept-charset", string.Empty),
			/* 16 */new HeaderField("accept-encoding", "gzip, deflate"),
			/* 17 */new HeaderField("accept-language", string.Empty),
			/* 18 */new HeaderField("accept-ranges", string.Empty),
			/* 19 */new HeaderField("accept", string.Empty),
			/* 20 */new HeaderField("access-control-allow-origin", string.Empty),
			/* 21 */new HeaderField("age", string.Empty),
			/* 22 */new HeaderField("allow", string.Empty),
			/* 23 */new HeaderField("authorization", string.Empty),
			/* 24 */new HeaderField("cache-control", string.Empty),
			/* 25 */new HeaderField("content-disposition", string.Empty),
			/* 26 */new HeaderField("content-encoding", string.Empty),
			/* 27 */new HeaderField("content-language", string.Empty),
			/* 28 */new HeaderField("content-length", string.Empty),
			/* 29 */new HeaderField("content-location", string.Empty),
			/* 30 */new HeaderField("content-range", string.Empty),
			/* 31 */new HeaderField("content-type", string.Empty),
			/* 32 */new HeaderField("cookie", string.Empty),
			/* 33 */new HeaderField("date", string.Empty),
			/* 34 */new HeaderField("etag", string.Empty),
			/* 35 */new HeaderField("expect", string.Empty),
			/* 36 */new HeaderField("expires", string.Empty),
			/* 37 */new HeaderField("from", string.Empty),
			/* 38 */new HeaderField("host", string.Empty),
			/* 39 */new HeaderField("if-match", string.Empty),
			/* 40 */new HeaderField("if-modified-since", string.Empty),
			/* 41 */new HeaderField("if-none-match", string.Empty),
			/* 42 */new HeaderField("if-range", string.Empty),
			/* 43 */new HeaderField("if-unmodified-since", string.Empty),
			/* 44 */new HeaderField("last-modified", string.Empty),
			/* 45 */new HeaderField("link", string.Empty),
			/* 46 */new HeaderField("location", string.Empty),
			/* 47 */new HeaderField("max-forwards", string.Empty),
			/* 48 */new HeaderField("proxy-authenticate", string.Empty),
			/* 49 */new HeaderField("proxy-authorization", string.Empty),
			/* 50 */new HeaderField("range", string.Empty),
			/* 51 */new HeaderField("referer", string.Empty),
			/* 52 */new HeaderField("refresh", string.Empty),
			/* 53 */new HeaderField("retry-after", string.Empty),
			/* 54 */new HeaderField("server", string.Empty),
			/* 55 */new HeaderField("set-cookie", string.Empty),
			/* 56 */new HeaderField("strict-transport-security", string.Empty),
			/* 57 */new HeaderField("transfer-encoding", string.Empty),
			/* 58 */new HeaderField("user-agent", string.Empty),
			/* 59 */new HeaderField("vary", string.Empty),
			/* 60 */new HeaderField("via", string.Empty),
			/* 61 */new HeaderField("www-authenticate", string.Empty)
		];

		private static Dictionary<string, int> STATIC_INDEX_BY_NAME = CreateMap();
		
		private static bool IsWriteProtected = false;
		private static bool IsUsedByHTTP2 = true;

		/// <summary>
		/// The number of header fields in the static table.
		/// </summary>
		/// <value>The length.</value>
		public static int Length { get { return STATIC_TABLE.Count; } }

		/// <summary>
		/// Empties StaticTable for use outside of HTTP/2.
		/// </summary>
		public static void UseStandaloneHPACK()
		{
			STATIC_TABLE.Clear();
			IsUsedByHTTP2 = false;
			STATIC_INDEX_BY_NAME = CreateMap();
		}

		/// <summary>
		/// Prevents modifying StaticTable once in use.
		/// </summary>
		public static void EnableWriteProtection()
		{
			IsWriteProtected = true;
		}

		/// <summary>
		/// Return the header field at the given index value.
		/// </summary>
		/// <returns>The entry.</returns>
		/// <param name="index">Index.</param>
		public static HeaderField GetEntry(int index)
		{
			return STATIC_TABLE[index - 1];
		}

		/// <summary>
		/// Returns the lowest index value for the given header field name in the static table.
		/// Returns -1 if the header field name is not in the static table.
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="name">Name.</param>
		public static int GetIndex(byte[] name)
		{
			string nameString = Encoding.UTF8.GetString(name);

			if (!STATIC_INDEX_BY_NAME.TryGetValue(nameString, out int value))
				return -1;

			return value;
		}

		/// <summary>
		/// Returns the index value for the given header field in the static table.
		/// Returns -1 if the header field is not in the static table.
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="name">Name.</param>
		/// <param name="value">Value.</param>
		public static int GetIndex(byte[] name, byte[] value)
		{
			int index = GetIndex(name);

			if (index == -1)
				return -1;

			// Note this assumes all entries for a given header field are sequential.
			while (index <= StaticTable.Length)
			{
				HeaderField entry = GetEntry(index);

				if (!HpackUtil.Equals(name, entry.Name))
					break;

				if (HpackUtil.Equals(value, entry.Value))
					return index;

				index++;
			}

			return -1;
		}

		/// <summary>
		/// create a map of header name to index value to allow quick lookup
		/// </summary>
		/// <returns>The map.</returns>
		private static Dictionary<string, int> CreateMap()
		{
			int length = STATIC_TABLE.Count;
			Dictionary<string, int> ret = new(length);

			// Iterate through the static table in reverse order to
			// save the smallest index for a given name in the map.
			for (int index=length; index>0; index--)
			{
				HeaderField entry = GetEntry(index);
				string name = Encoding.UTF8.GetString(entry.Name);
				ret[name] = index;
			}

			return ret;
		}

		public static void Add(string name, string value)
		{
			if (IsUsedByHTTP2)
				throw new InvalidOperationException(
					"Illegal operation: Static table in use by HTTP/2."
				);
			
			if (IsWriteProtected)
				throw new InvalidOperationException(
					"Illegal operation: Encoder/Decoder already initialised."
				);

			STATIC_TABLE.Add(new HeaderField(name, value));
			STATIC_INDEX_BY_NAME = CreateMap();
		}

		public static void Remove(byte[] name, byte[] value)
		{
			int index = GetIndex(name, value);

			if (index == -1)
				throw new IndexOutOfRangeException("Header field not found.");

			Remove(index);
		}

		public static void Remove(byte[] name)
		{
			int index = GetIndex(name);

			if (index == -1)
				throw new IndexOutOfRangeException("Header field not found.");

			Remove(index);
		}

		public static void Remove(int index)
		{
			if (IsUsedByHTTP2)
				throw new InvalidOperationException(
					"Illegal operation: Static table in use by HTTP/2."
				);
			
			if (IsWriteProtected)
				throw new InvalidOperationException(
					"Illegal operation: Encoder/Decoder already initialised."
				);

			STATIC_TABLE.RemoveAt(index);
			STATIC_INDEX_BY_NAME = CreateMap();
		}

		public static void Clear()
		{
			if (IsUsedByHTTP2)
				throw new InvalidOperationException(
					"Illegal operation: Static table in use by HTTP/2."
				);
			
			if (IsWriteProtected)
				throw new InvalidOperationException(
					"Illegal operation: Encoder/Decoder already initialised."
				);

			STATIC_TABLE.Clear();
			STATIC_INDEX_BY_NAME = CreateMap();
		}
	}
}
