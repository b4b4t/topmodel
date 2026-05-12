//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::restaurant::Restaurant;

/// Employé du restaurant
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Employe {
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
    pub restaurant: Restaurant,
}
