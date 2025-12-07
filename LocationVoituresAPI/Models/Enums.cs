namespace LocationVoituresAPI.Models;

public enum TypeUtilisateur
{
    ADMINISTRATEUR,
    EMPLOYE,
    CLIENT
}

public enum StatutLocation
{
    EN_ATTENTE,
    CONFIRMEE,
    EN_COURS,
    TERMINEE,
    ANNULEE
}

public enum ModePaiement
{
    CARTE_CREDIT,
    VIREMENT,
    ESPECES
}

public enum StatutPaiement
{
    EN_ATTENTE,
    VALIDE,
    ECHOUE,
    REMBOURSE
}

public enum TypeEntretien
{
    REVISION_PERIODIQUE,
    REPARATION_MECANIQUE,
    REMPLACEMENT_PNEUS,
    VIDANGE,
    CONTROLE_TECHNIQUE,
    NETTOYAGE,
    REPARATION_CARROSSERIE
}

public enum StatutEntretien
{
    PLANIFIE,
    EN_COURS,
    TERMINE,
    ANNULE
}

public enum TypeRapport
{
    LOCATIONS_MENSUELLES,
    LOCATIONS_PERIODE,
    REVENUS_FINANCIERS,
    REVENUS_PERIODE,
    PERFORMANCE_VEHICULES,
    VEHICULES,
    ACTIVITE_CLIENTS,
    ENTRETIENS_PENDANT,
    ENTRETIENS,
    STATISTIQUES_GLOBALES
}

public enum FormatRapport
{
    PDF,
    EXCEL,
    CSV
}

