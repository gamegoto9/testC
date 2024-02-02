using System.ComponentModel;

namespace DotnetAPI.Dtos
{
    public partial class SearchDto
    {
        [DefaultValue("all")]
        public string Fno { get; set; }
        [DefaultValue("0")]
        public string MkID { get; set; }
        [DefaultValue("0")]
        public string MdID { get; set; }
        [DefaultValue("0")]
        public string BdID { get; set; }
        [DefaultValue("0")]
        public string TaID { get; set; }
        [DefaultValue("0")]
        public string Bt1 { get; set; }
        [DefaultValue("0")]
        public string Bt2 { get; set; }
        [DefaultValue("0")]
        public string Yr1 { get; set; }
        [DefaultValue("0")]
        public string Yr2 { get; set; }
        [DefaultValue("0")]
        public string Tys { get; set; }
        [DefaultValue("0")]
        public string Dpmt { get; set; }
        [DefaultValue("N")]
        public string IsDpmt { get; set; }
        [DefaultValue("b")]
        public string Gr { get; set; }
        [DefaultValue("n")]
        public string Gs { get; set; }
        [DefaultValue("0")]
        public string Cl { get; set; }
        [DefaultValue("0")]
        public string Jv { get; set; }
        [DefaultValue("y")]
        public string Sort { get; set; }

        public SearchDto()
        {
            if (Fno == null)
            {
                Fno = "all";
            }
            if (MkID == null) {
                MkID = "0";
            }
            if (MdID == null) {
                MdID = "0";
            }
            if(BdID == null) {
                BdID = "0";
            }
            if(TaID == null) {
                TaID = "0";
            }
            if(Bt1 == null){
                Bt1 = "0";
            }
            if(Bt2 == null){
                Bt2 = "0";
            }
            if(Yr1 == null){
                Yr1 = "0";
            }
            if(Yr2 == null){
                Yr2 = "0";
            }
            if(Tys == null){
                Tys = "0";
            }
            if(Dpmt == null){
                Dpmt = "0";
            }
            if(IsDpmt == null){
                IsDpmt = "N";
            }
            if(Gr == null){
                Gr = "b";
            }
            if(Gs == null){
                Gs = "n";
            }
            if(Cl == null){
                Cl = "0";
            }
            if(Jv == null){
                Jv = "0";
            }
            if(Sort == null){
                Sort = "y";
            }

        }
    }



}