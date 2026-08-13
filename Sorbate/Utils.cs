namespace Sorbate;

public static class Utils {
    extension<T>(IReadOnlyList<(string path, T value)> list) {
        public bool TryGetValue(string target, out T? val) {
            val = list.FirstOrDefault(x => x.path == target).value;
            return val is not null;
        }
    }
}