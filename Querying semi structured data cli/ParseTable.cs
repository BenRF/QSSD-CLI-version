using System;
using System.Collections.Generic;

namespace Querying_semi_structured_data_cli {
    class ParseTable {
        private List<ParseColumn> columns;
        private string sortedBy;
        private readonly string name;

        public ParseTable(List<List<ParseColumn>> content) {
            sortedBy = "";
            columns = new List<ParseColumn>();
            foreach (object header in content[0]) {
                NewCol(header.ToString());
            }
            for (int y = 1; y < content.Count; y++) {
                for (int x = 0; x < content.Count; x++) {
                    columns[x].AddContent(content[y][x]);
                }
            }
            PerformChecks();
        }

        public ParseTable(ParseTable p1, ParseTable p2) {
            List<Link> links = p1.GetLinks(p2);
            SetupFromTwoTables(p1, p2, links);
        }

        public ParseTable(ParseTable p1, ParseTable p2, List<Link> links) {
            SetupFromTwoTables(p1, p2, links);
        }

        private void SetupFromTwoTables(ParseTable p1, ParseTable p2, List<Link> links) {
            List<ParseColumn> t1 = new List<ParseColumn>();
            List<ParseColumn> t2 = new List<ParseColumn>();
            foreach (Link l in links) {
                int[] cols = { p1.GetColIdFromName(l.GetFirstCol()), p2.GetColIdFromName(l.GetSecondCol()) };
                t1.Add(p1.GetCol(p1.GetColIdFromName(l.GetFirstCol())));
                t2.Add(p2.GetCol(p2.GetColIdFromName(l.GetSecondCol())));
            }
            if (this.IsUnique(t1) && !this.IsUnique(t2)) {
                ParseTable temp = p2;
                p2 = p1;
                p1 = temp;
                links = p1.GetLinks(p2);
            }
            this.columns = new List<ParseColumn>();
            foreach (ParseColumn c in p1.GetColumns()) {
                this.NewCol(c);
            }
            List<int[]> linkedT2Cols = new List<int[]>();
            foreach (Link l in links) {
                linkedT2Cols.Add(new int[] { p2.GetColIdFromName(l.GetSecondCol()), p1.GetColIdFromName(l.GetFirstCol()) });
            }
            bool match;
            foreach (ParseColumn c in p2.GetColumns()) {
                match = false;
                foreach (int[] l in linkedT2Cols) {
                    if (l[0] == c.GetId()) {
                        match = true;
                        break;
                    }
                }
                if (!match) {
                    this.NewCol(c.GetName());
                }
            }
            HashSet<object> tab2Set = p2.GetCol(p2.GetColIdFromName(links[0].GetSecondCol())).GetContentAsSet();
            object o1, o2;
            int temp1, temp2;
            this.SortByColId(this.GetColIdFromName(links[0].GetFirstCol()));
            p2.SortByColId(p2.GetColIdFromName(links[0].GetSecondCol()));
            int upTo = 0;
            static bool isNull(object x) => x == null;
            for (int r = 0; r < this.GetRowCount(); r++) {
                List<object> row;
                for (int r2 = 0; r2 < p2.GetRowCount(); r++) {
                    row = p2.GetRow(r2);
                    match = true;
                    foreach (Link l in links) {
                        temp1 = p1.GetColIdFromName(l.GetFirstCol());
                        temp2 = p2.GetColIdFromName(l.GetSecondCol());
                        o1 = row[temp2];
                        o2 = this.GetRow(r)[temp1];
                        if (o1 != null && o2 != null && !o1.Equals(o2)) {
                            match = false;
                            break;
                        }
                    }
                    if (match) {
                        upTo = r2;
                        tab2Set.Remove(row[p2.GetColIdFromName(links[0].GetSecondCol())]);
                        foreach (int[] i in linkedT2Cols) {
                            if (this.IsCellEmpty(i[1],r)) {
                                this.SetCell(i[1], r, row[i[0]]);
                            }
                            row[i[0]] = null;
                        }
                        row.RemoveAll(isNull);
                        for (int c = 0; c < row.Count; c++) {
                            this.SetCell(p1.GetColumnCount() + c, r, row[c]);
                        }
                        break;
                    }
                }
            }
            if (tab2Set.Count > 0) {
                foreach (object o in tab2Set) {
                    List<object> rowToAdd = p2.FindRowByObject(p2.GetColIdFromName(links[0].GetSecondCol()), o);
                    this.NewRow();
                    foreach (int[] i in linkedT2Cols) {
                        this.SetCell(i[1], this.GetRowCount() - 1, rowToAdd[i[0]]);
                    }
                    foreach (int[] i in linkedT2Cols) {
                        rowToAdd[i[0]] = null;
                    }
                    rowToAdd.RemoveAll(isNull);
                    for (int c = 0; c < rowToAdd.Count; c++) {
                        this.SetCell(p1.GetColumnCount() + c, this.GetRowCount() - 1, rowToAdd[c]);
                    }
                }
            }
            this.sortedBy = "";
            this.PerformChecks();
        }

        public ParseTable(ParseTable table) {
            this.name = table.name;
            this.columns = new List<ParseColumn>();
            this.sortedBy = table.sortedBy;
            foreach (ParseColumn c in table.columns) {
                this.columns.Add(new ParseColumn(c));
            }
        }

        public 

        int GetColIdFromName(String name) {
            int result = -1;
            foreach (ParseColumn c in columns) {
                if (name.Equals(c.GetName())) {
                    result = c.GetId();
                    break;
                }
            }
            return result;
        }

        void SortByColName(string name) {
            SortByColId(GetColIdFromName(name));
        }

        private String GetNameById(int id) { return this.GetCol(id).GetName(); }

        private void SortByColId(int columnNumber) {
            if (columns.Count > 0) {
                sortedBy = GetNameById(columnNumber);
                QuickSort(columnNumber, 0, columns[columnNumber].Size() - 1);
            }
        }

        private void QuickSort(int columnNumber, int low, int high) {
            if (low >= high) {
                return;
            }
            object pivot = GetCell(columnNumber, high);
            int cnt = low;
            for (int i = low; i <= high; i++) {
                if (ObjectLessThanOrEqualTo(GetCell(columnNumber,i),pivot)) {
                    SwapRows(cnt, i);
                    cnt++;
                }
            }
            QuickSort(columnNumber, low, cnt - 2);
            QuickSort(columnNumber, cnt, high);
        }

        private bool ObjectLessThanOrEqualTo(object o1, object o2) {
            if (o1 is string && o2 is string) {
                String s1 = (String)o1;
                String s2 = (String)o2;
                return s1.CompareTo(s2) <= 0;
            } else if (o1 is int && o2 is int) {
                int i1 = (int)o1;
                int i2 = (int)o2;
                return i1.CompareTo(i2) <= 0;
            } else {
                return false;
            }
        }

        private void SwapRows(int rowPos1,int rowPos2) {
            foreach (ParseColumn c in columns) {
                c.Swap(rowPos1, rowPos2);
            }
        }

        public List<List<object>> GetContent() {
            List<List<object>> content = new List<List<object>>();
            for (int i = 0; i < GetRowCount(); i++) {
                content[i] = GetRow(i);
            }
            return content;
        }

        public List<Link> GetLinks(ParseTable p2) {
            List<ParseColumn> p1c = columns;
            List<ParseColumn> p2c = p2.columns;
            List<Link> links = new List<Link>();
            foreach (ParseColumn c1 in p1c) {
                foreach (ParseColumn c2 in p2c) {
                    bool type = c1.CheckType(c2);
                    if (type || c1.IsEmpty() || c2.IsEmpty()) {
                        int[] content = c1.CheckContent(c2);
                        bool name = c1.GetName().Equals(c2.GetName());
                        if (Math.Max(content[0],content[1]) > 85 || name) {
                            links.Add(new Link(c1.GetName(), c2.GetName(),name, content[0], content[1]));
                        }
                    }
                }
            }
            bool removed = false;
            int x = 0;
            if (links.Count > 1) {
                while (x < links.Count) {
                    Link link = links[x];
                    for (int y = 0; y < links.Count; y++) {
                        Link link2 = links[y];
                        if (link.Equal(link2)) {
                            if (link.Stronger(link2)) {
                                links.RemoveAt(y);
                                removed = true;
                            } else {
                                links.RemoveAt(x);
                                removed = true;
                                break;
                            }
                        }
                    }
                    if (removed) {
                        removed = false;
                        x = 0;
                    } else {
                        x++;
                    }
                }
            }
            return links;
        }

        private String[] GetRowAsString(int rowNum) {
            List<Object> r = GetRow(rowNum);
            String[] row = new String[r.Count];
            for (int y = 0; y < r.Count; y++) {
                if (r[y] != null) {
                    row[y] = r[y].ToString();
                } else {
                    row[y] = "";
                }
            }
            return row;
        }

        private void PerformChecks() {
            foreach (ParseColumn c in columns) {
                c.PerformChecks();
            }
        }

        private bool IsUnique(List<ParseColumn> columns) {
            List<List<object>> values = new List<List<object>>();
            for (int i = 0; i < columns[0].Size(); i++) {
                List<object> row = new List<object>();
                foreach (ParseColumn c in columns) {
                    row.Add(c.Get(i));
                }
                values.Add(row);
            }
            HashSet<object> results = new HashSet<object>();
            return results.Count == values.Count;
        }

        private List<object> FindRowByObject(int columnId, object searchFor) {
            ParseColumn selected = GetCol(columnId);
            int row = selected.FindRowByObject(searchFor);
            return GetRow(row);
        }

        private void NewRow() {
            foreach (ParseColumn c in columns) {
                c.AddContent(null);
            }
        }

        private void NewCol(String name) {
            columns.Add(new ParseColumn(name, columns.Count));
            Normalise();
        }

        private void NewCol(ParseColumn pC) {
            columns.Add(new ParseColumn(pC, columns.Count));
        }

        private ParseColumn GetCol(int i) { return columns[i]; }

        private List<ParseColumn> GetColumns() { return columns; }

        public List<object> GetRow(int rowNum) {
            List<object> r = new List<object>();
            foreach (ParseColumn c in columns) {
                r.Add(c.Get(rowNum));
            }
            return r;
        }

        private void Normalise() {
            int max = 0;
            foreach (ParseColumn c in columns) {
                if (c.Size() > max) {
                    max = c.Size();
                }
            }
            foreach (ParseColumn c in columns) {
                c.Normalise(max);
            }
        }

        public int GetRowCount() {
            return columns[0].Size();
        }

        private object GetCell(int column, int row) {
            return columns[column].Get(row);
        }

        private void SetCell(int column, int row, object o) {
            columns[column].Set(row, o);
        }

        private int GetColumnCount() { return columns.Count; }

        private bool IsCellEmpty(int column, int row) {
            return GetCell(column, row) == null;
        }
    }
}
