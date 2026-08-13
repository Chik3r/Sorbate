namespace Sorbate;

public static class Utils {
    extension<T>(IReadOnlyList<(string path, T value)> list) {
        public bool TryGetValue(string target, out T? val) {
            var tmp = list.FirstOrDefault(x => x.path == target, ("null.null", default)!);

            if (tmp.path == "null.null") {
                val = default;
                return false;
            }

            val = tmp.value;
            return true;
        }
    }
}