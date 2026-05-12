//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Classe de base représentant une personne
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct Personne {
    /// Identifiant de la personne
    pub id: i32,

    /// Nom de la personne
    pub nom: String,

    /// Prénom de la personne
    pub prenom: String,

    /// Département de résidence de la personne.
    pub departement_code: Option<String>,
}
