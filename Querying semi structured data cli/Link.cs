using System;
using System.Collections.Generic;
using System.Text;

namespace Querying_semi_structured_data_cli {
    class Link {
        private int col1Overlap, col2Overlap;
        private String col1, col2;
        private bool sameName;

        public Link(String col1, String col2, bool name, int col1Overlap, int col2Overlap) {
            this.col1 = col1;
            this.col2 = col2;
            sameName = name;
            this.col1Overlap = col1Overlap;
            this.col2Overlap = col2Overlap;
        }

        public bool Equal(Link l2) {
            bool col1Match = this.col1.Equals(l2.col1);
            bool col2Match = this.col2.Equals(l2.col2);
            return (col1Match && !col2Match) || (!col1Match && col2Match);
        }

        public bool Stronger(Link l2) {
            int thisOverlap = this.col1Overlap + this.col2Overlap;
            int otherOverlap = l2.col1Overlap + l2.col2Overlap;
            bool sameNameVal = this.sameName == l2.sameName;

            bool higherSimilarity = thisOverlap > otherOverlap;
            bool sameOrHigherSimilarity = thisOverlap >= otherOverlap;
            bool overThreshold = Math.Max(this.col1Overlap, this.col2Overlap) > 95;
            bool otherHasNameMatch = !this.sameName && l2.sameName;
            bool thisHasNameMatch = this.sameName && !l2.sameName;
            return (sameNameVal && higherSimilarity) || (overThreshold && otherHasNameMatch) || (thisHasNameMatch && sameOrHigherSimilarity);
        }

        public override string ToString() {
            return "[" + col1 + "," + col2 + "," + sameName + "," + col1Overlap + "," + col2Overlap + "]";
        }

        public String GetFirstCol() { return col1; }

        public String GetSecondCol() { return col2; }
    }
}
