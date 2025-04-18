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

namespace hpack
{
	/// <summary>
	/// The DynamicTable class.
	/// </summary>
	public class DynamicTable
	{
		// a circular queue of header fields
		HeaderField[] headerFields;
		int head;
		int tail;
		private int size;
		private int capacity = -1;
		// ensure setCapacity creates the array

		/// <summary>
		/// The Capacity
		/// </summary>
		/// <value></value>
		public int Capacity { get { return this.capacity; } }

		/// <summary>
		/// The Size
		/// </summary>
		/// <value></value>
		public int Size { get { return this.size; } }

		/// <summary>
		/// Creates a new dynamic table with the specified initial capacity.
		/// </summary>
		/// <param name="initialCapacity">Initial capacity.</param>
		public DynamicTable(int initialCapacity)
		{
			this.SetCapacity(initialCapacity);
		}

		/// <summary>
		/// Return the number of header fields in the dynamic table.
		/// </summary>
		public int Length()
		{
			int length = 0;

			if (this.head < this.tail)
				length = this.headerFields.Length - this.tail + this.head;

			else

				length = this.head - this.tail;

			return length;
		}

		/// <summary>
		/// Return the current size of the dynamic table.
		/// This is the sum of the size of the entries.
		/// </summary>
		/// <returns>The size.</returns>
		public int GetSize()
		{
			return this.size;
		}

		/// <summary>
		/// Return the maximum allowable size of the dynamic table.
		/// </summary>
		/// <returns>The capacity.</returns>
		public int GetCapacity()
		{
			return capacity;
		}

		/// <summary>
		/// Return the header field at the given index.
		/// The first and newest entry is always at index 1,
		/// and the oldest entry is at the index length().
		/// </summary>
		/// <returns>The entry.</returns>
		/// <param name="index">Index.</param>
		public HeaderField GetEntry(int index)
		{
			if (index <= 0 || index > this.Length())
				throw new HPackIndexOutOfRangeException();

			int i = this.head - index;

			if (i < 0)
				return this.headerFields[i + this.headerFields.Length];

			else
				return this.headerFields[i];
		}

		/// <summary>
		/// Add the header field to the dynamic table.
		/// Entries are evicted from the dynamic table until the size of the table
		/// and the new header field is less than or equal to the table's capacity.
		/// If the size of the new entry is larger than the table's capacity,
		/// the dynamic table will be cleared.
		/// </summary>
		/// <param name="header">Header.</param>
		public void Add(HeaderField header)
		{
			int headerSize = header.Size;

			if (headerSize > this.capacity)
			{
				this.Clear();
				return;
			}

			while (this.size + headerSize > this.capacity)
				this.Remove();

			this.headerFields[this.head++] = header;
			this.size += header.Size;

			if (this.head == this.headerFields.Length)
				this.head = 0;
		}

		/// <summary>
		/// Remove and return the oldest header field from the dynamic table.
		/// </summary>
		public HeaderField Remove()
		{
			HeaderField removed = this.headerFields[this.tail];

			if (removed == null)
				return null;

			this.size -= removed.Size;
			this.headerFields[this.tail++] = null;

			if (this.tail == this.headerFields.Length)
				this.tail = 0;

			return removed;
		}

		/// <summary>
		/// Remove all entries from the dynamic table.
		/// </summary>
		public void Clear()
		{
			while (this.tail != this.head)
			{
				this.headerFields[this.tail++] = null;

				if (this.tail == this.headerFields.Length)
					this.tail = 0;
			}
			this.head = 0;
			this.tail = 0;
			this.size = 0;
		}

		/// <summary>
		/// Set the maximum size of the dynamic table.
		/// Entries are evicted from the dynamic table until the size of the table
		/// is less than or equal to the maximum size.
		/// </summary>
		/// <param name="capacity">Capacity.</param>
		public void SetCapacity(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentException("Illegal Capacity: " + capacity);

			// initially capacity will be -1 so init won't return here
			if (this.capacity == capacity)
				return;

			this.capacity = capacity;

			if (capacity == 0)
				this.Clear();

			else
			{
				// initially size will be 0 so remove won't be called
				while (this.size > capacity)
					this.Remove();
			}

			int maxEntries = capacity / HeaderField.HEADER_ENTRY_OVERHEAD;

			if (capacity % HeaderField.HEADER_ENTRY_OVERHEAD != 0)
				maxEntries++;

			// check if capacity change requires us to reallocate the array
			if (this.headerFields != null && this.headerFields.Length == maxEntries)
				return;

			HeaderField[] tmp = new HeaderField[maxEntries];

			// initially length will be 0 so there will be no copy
			int len = this.Length();
			int cursor = this.tail;

			for (int i=0; i<len; i++)
			{
				HeaderField entry = this.headerFields[cursor++];
				tmp[i] = entry;

				if (cursor == this.headerFields.Length)
					cursor = 0;
			}

			this.tail = 0;
			this.head = this.tail + len;
			this.headerFields = tmp;
		}
	}
}
