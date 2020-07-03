﻿using Xamarin.Essentials;

namespace Groundsman {
    public static class AppConstants {
        private static readonly string DATA_PATH = FileSystem.AppDataDirectory + "/";
        private static readonly string FEATURES_FILENAME = "locations.json";
        private static readonly string LOG_FILENAME = "log.csv";

        private static readonly string GENERATED_FEATURES_FILENAME = "locationsAutoGenerated.json";

        public static readonly string NEW_ENTRY_ID = "-1";
        public static readonly string FEATURES_FILE = DATA_PATH + FEATURES_FILENAME;
        public static readonly string LOG_FILE = DATA_PATH + LOG_FILENAME;
    }
}
