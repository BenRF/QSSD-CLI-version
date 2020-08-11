using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Querying_semi_structured_data_cli {
    class ParseColumn {
        private readonly string name;
        private int id;
        private readonly ArrayList content;
        private bool uniqueValues;
        private int numOfUniqueVals;
        private bool sameType;
        private bool empty;
        //private Expression format;
        private ArrayList errors;

        public ParseColumn(String name, int id) {
            this.name = name;
            this.id = id;
            content = new ArrayList();
            errors = new ArrayList();
        }

        public ParseColumn(ParseColumn column, int id) {
            name = column.name;
            this.id = id;
            content = new ArrayList(column.content);
            uniqueValues = column.uniqueValues;
            numOfUniqueVals = column.numOfUniqueVals;
            sameType = column.sameType;
            //this.format = column.format;
            errors = new ArrayList();
        }

        public ParseColumn(ParseColumn column) {
            name = column.name;
            id = column.id;
            content = new ArrayList(column.content);
            uniqueValues = column.uniqueValues;
            numOfUniqueVals = column.numOfUniqueVals;
            sameType = column.sameType;
            //this.format = column.format;
            errors = (ArrayList)column.errors.Clone();
            empty = column.empty;
        }

        public void AddContent(Object o) {
            content.Add(o);
        }

        public void Normalise(int newSize) {
            while (content.Count < newSize) {
                AddContent(null);
            }
        }

        public void Set(int rowNum, Object newObj) {
            content[rowNum] = newObj;
        }

        public void PerformChecks() {
            IsEmpty();
            if (!empty) {
                //this.findExpressions();
                CheckUnique();
                CheckTypes();
                HasEmpty();
            } else {
                //this.format = null;
                uniqueValues = false;
                sameType = true;
            }
        }

        private void HasEmpty() {
            int count = 0;
            ArrayList nulls = new ArrayList();
            for (int i = 0; i < this.content.Count; i++) {
                if (this.content[i] == null) {
                    count++;
                    nulls.Add(i);
                }
            }
            //if (count != 0) {
            //    this.errors.Add(new MissingValue(this.id, nulls, this.name));
            //}
        }

        private void CheckUnique() {
            HashSet<object> content = new HashSet<object>((IEnumerable<object>)this.content);
            bool hadNull = content.Remove(null);
            int nullCount = 0;
            if (hadNull) {
                foreach (object o in this.content) {
                    if (o == null) {
                        nullCount++;
                    }
                }
            }
            numOfUniqueVals = content.Count;
            uniqueValues = content.Count == this.content.Count - nullCount;
            /*if (content.size() > (this.content.size() - nullCount) * 0.85 && content.size() < this.content.size() - nullCount) {
                ArrayList<Integer> flags = new ArrayList<>();
                Map<Object, ArrayList<Integer>> valsWithPos = new HashMap<>();
                for (int i = 0; i < this.content.size(); i++) {
                    Object o = this.content.get(i);
                    if (valsWithPos.containsKey(o)) {
                        valsWithPos.get(o).add(i);
                    } else {
                        ArrayList<Integer> pos = new ArrayList<>();
                        pos.add(i);
                        valsWithPos.put(o, pos);
                    }
                }
                for (ArrayList<Integer> o: valsWithPos.values()) {
                    if (o.size() > 1) {
                        flags.addAll(o);
                    }
                }
                this.errors.add(new NearlyUnique(this.id, flags, this.name));
            }*/
        }

        /*Expression getFormat() {
            if (this.getFormat != null) {
                return this.getFormat;
            } else {
                return null;
            }
        }*/

        ArrayList GetProblems() {
            return errors;
        }

        private void CheckTypes() {
            IDictionary types = new Dictionary<Type, int>();
            foreach (Object o in content) {
                if (o != null) {
                    if (types.Contains(o.GetType())) {
                        types[o.GetType()] = (int)types[o.GetType()] + 1;
                    } else {
                        types.Add(o.GetType(), 1);
                    }
                }
            }
            /*int size = this.content.size();
            ArrayList<Integer> flags = new ArrayList<>();
            for (Map.Entry<Class, Integer> entry : types.entrySet()) {
                if (entry.getValue() >= size * 0.85 && entry.getValue() < size) {
                    for (int i = 0; i < this.content.size(); i++) {
                        if (this.content.get(i) != null) {
                            if (!this.content.get(i).getClass().equals(entry.getKey())) {
                                flags.add(i);
                            }
                        }
                    }
                }
            }
            if (flags.size() > 0) {
                this.errors.add(new MixedTypes(this.id, flags, this.name));
            }*/
            sameType = types.Count == 1;
        }

        public override string ToString() { 
            return name;
        }

        public int Size() { return content.Count; }

        public String GetName() { return name; }

        public int GetId() { return id; }

        void SetId(int newId) { id = newId; }

        String GetAttributes() {
            String output = "";
            bool first = true;
            if (!empty) {
                if (uniqueValues) {
                    output += "Unique";
                    first = false;
                }
                if (sameType) {
                    if (first) {
                        int pos = 0;
                        while (pos < content.Count && content[pos] == null) {
                            pos++;
                        }
                        if (pos < content.Count) {
                            output = $"{output},{content[pos].GetType().Name}";
                        }
                    } else {
                        if (content[0] != null) {
                            output = $"{output},{content[0].GetType().Name}";
                        }
                    }
                }
            } else {
                return "empty";
            }
            return output;
        }

        public Object Get(int row) {
            return content[row];
        }

        public int FindRowByObject(Object o) {
            for (int i = 0; i < content.Count; i++) {
                if (content[i].Equals(o)) {
                    return i;
                }
            }
            return -1;
        }

        public HashSet<Object> GetContentAsSet() {
            return new HashSet<Object>((IEnumerable<object>)content);
        }

        public void Swap(int rowOne, int rowTwo) {
            Object temp = content[rowOne];
            content[rowOne] = content[rowTwo];
            content[rowTwo] = temp;
        }

        public bool IsEmpty() {
            bool isEmpty = true;
            foreach (Object o in content) {
                if (o != null) {
                    isEmpty = false;
                    break;
                }
            }
            empty = isEmpty;
            return isEmpty;
        }

        public bool CheckType(ParseColumn otherColumn) => sameType && otherColumn.sameType;

        public int[] CheckContent(ParseColumn otherColumn) {
            int[] results = new int[2];
            IEnumerable<Object> c1 = from contents in GetContentAsSet().Except(otherColumn.GetContentAsSet()) select content;
            IEnumerable<Object> c2 = from contents in otherColumn.GetContentAsSet().Except(GetContentAsSet()) select content;
            results[0] = (content.Count - c1.Count()) / content.Count * 100;
            results[1] = (int)Math.Round(((float)otherColumn.GetContentAsSet().Count - c2.Count()) / otherColumn.GetContentAsSet().Count * 100);
            return results;
        }

        public override bool Equals(object obj) {
            if (obj is ParseColumn pc) {
                if (name.Equals(pc.GetName()) && Size() == pc.Size()) {
                    for (int i = 0; i < Size(); i++) {
                        if ((Get(i) == null && pc.Get(i) != null) || (Get(i) != null && pc.Get(i) == null)) {
                            break;
                        } else if ((Get(i) != null || pc.Get(i) != null) && !Get(i).Equals(pc.Get(i))) {
                            break;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
