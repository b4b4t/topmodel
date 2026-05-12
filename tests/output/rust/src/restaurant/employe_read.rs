//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// Détail d'un employé en lecture
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct EmployeRead {
    /// Identifiant de la personne
    pub id: i32,

    /// Nom de la personne
    pub nom: String,

    /// Prénom de la personne
    pub prenom: String,

    /// Département de résidence de la personne.
    pub departement_code: Option<String>,

    /// Numéro de téléphone de l'employé.
    pub telephone: Option<String>,

    /// Date de naissance
    pub date_naissance: Option<NaiveDateTime>,

    /// Matricule de l'employé
    pub matricule: String,

    /// Date d'embauche
    pub date_embauche: NaiveDateTime,

    /// Salaire de l'employé
    pub salaire: Option<Decimal>,

    /// Restaurant où travaille l'employé
    pub restaurant_id: i32,
}
