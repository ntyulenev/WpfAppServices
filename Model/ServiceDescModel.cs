using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppServices.Model {
    class ServiceDescModel {

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string State { get; set; }

        public string Account { get; set; }

        #region Override Equals to compare by value

        public override bool Equals(object obj) => this.Equals(obj as ServiceDescModel);

        public bool Equals(ServiceDescModel s) {
            if (s is null) {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, s)) {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != s.GetType()) {
                return false;
            }

            bool isNameEquals =
                DisplayName == s.DisplayName ||
                (DisplayName != null && s.DisplayName != null && // avoid ArgumentNullException in string.Compare
                string.Compare(DisplayName, s.DisplayName, true, CultureInfo.InvariantCulture) == 0); // windows service name case insantive

            return
                isNameEquals &&
                Description == s.Description &&
                State == s.State &&
                Account == s.Account;
        }

        // Name was transformed ToLowerInvariant due to windows service name case insantive (GetHashCode should be same for equals objects)
        public override int GetHashCode() => (DisplayName.ToLowerInvariant(), Description, State, Account).GetHashCode();

        public static bool operator ==(ServiceDescModel lhs, ServiceDescModel rhs) {
            if (lhs is null) {
                if (rhs is null) {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ServiceDescModel lhs, ServiceDescModel rhs) => !(lhs == rhs);

        #endregion Override Equals

    }
}
