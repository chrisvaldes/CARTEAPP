namespace SYSGES_MAGs.Models
{
    class Periode
    {
        public DateTimeOffset Debut { get; set; }
        public DateTimeOffset Fin { get; set; }

        public Periode(DateTimeOffset debut, DateTimeOffset fin)
        {
            Debut = debut;
            Fin = fin;
        }
    }
}
