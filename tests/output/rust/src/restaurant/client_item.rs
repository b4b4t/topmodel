//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Détail d'un client en liste
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ClientItem {
    /// Identifiant de la personne
    pub id: i32,

    /// Nom de la personne
    pub nom: String,

    /// Prénom de la personne
    pub prenom: String,

    /// Nom complet du client (calculé)
    pub nom_complet: Option<String>,
}
