using UnityEngine;

namespace Data
{
    /// <summary>
    /// Simple PlayerPrefs-backed storage that remembers which catalog items were unlocked by the player.
    /// </summary>
    public static class PurchaseStateStorage
    {
        private const string CharactersKeyPrefix = "ff.purchase.characters.";
        private const string RedeemCodesKeyPrefix = "ff.purchase.redeemcodes.";

        public static bool GetCharacterIsPaid(Characters character, bool defaultValue) =>
            GetState(CharactersKeyPrefix, BuildCharacterIdentifier(character), defaultValue);

        public static void SetCharacterIsPaid(Characters character, bool isPaid) =>
            SetState(CharactersKeyPrefix, BuildCharacterIdentifier(character), isPaid);

        public static bool GetRedeemCodeIsPaid(RedeemCodes redeemCode, bool defaultValue) =>
            GetState(RedeemCodesKeyPrefix, BuildRedeemCodeIdentifier(redeemCode), defaultValue);

        public static void SetRedeemCodeIsPaid(RedeemCodes redeemCode, bool isPaid) =>
            SetState(RedeemCodesKeyPrefix, BuildRedeemCodeIdentifier(redeemCode), isPaid);

        private static bool GetState(string prefix, string identifier, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return fallback;

            var key = prefix + identifier;
            return PlayerPrefs.GetInt(key, fallback ? 1 : 0) == 1;
        }

        private static void SetState(string prefix, string identifier, bool isPaid)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return;

            var key = prefix + identifier;
            PlayerPrefs.SetInt(key, isPaid ? 1 : 0);
            PlayerPrefs.Save();
        }

        private static string BuildCharacterIdentifier(Characters character)
        {
            if (character == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(character.name))
                return character.name.Trim();

            if (!string.IsNullOrWhiteSpace(character.tagline))
                return character.tagline.Trim();

            if (!string.IsNullOrWhiteSpace(character.image))
                return character.image.Trim();

            return string.Empty;
        }

        private static string BuildRedeemCodeIdentifier(RedeemCodes redeemCode)
        {
            if (redeemCode == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(redeemCode.code))
                return redeemCode.code.Trim();

            if (!string.IsNullOrWhiteSpace(redeemCode.desc))
                return redeemCode.desc.Trim();

            return string.Empty;
        }
    }
}
