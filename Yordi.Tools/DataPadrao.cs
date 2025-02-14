namespace Yordi.Tools
{
    public static class DataPadrao
    {
        /// <summary>
        /// Mesmo estando o servidor configurado para qualquer fuso horário, obtém-se o horário de Brasília
        /// </summary>
        /// <returns></returns>
        public static DateTime Brasilia
        {
            get
            {
                DateTime timeUtc = DateTime.UtcNow;
                try
                {
                    TimeZoneInfo kstZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"); // Brasilia/BRA
                    DateTime dateTimeBrasilia = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, kstZone);
                    return dateTimeBrasilia;
                }
                catch
                {
                    return timeUtc;
                }
            }
        }
        public static DateTime Maquina => DateTime.Now;
        public static string DataBrasiliaToMSSQL
            => string.Format("{0} {1:g}", Brasilia.ToString("yyyyMMdd"), Brasilia.TimeOfDay);

        /// <summary>
        /// Para corresponder a Data mínima no MySQL
        /// new DateTime(1900, 1, 1)
        /// </summary>
        public static DateTime MinValue => new DateTime(1900, 1, 1);
    }
}
