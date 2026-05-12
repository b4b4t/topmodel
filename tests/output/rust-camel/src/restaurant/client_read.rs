//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Détail d'un client en lecture
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct ClientRead {
    /// Identifiant de la personne
    pub id: i32,

    /// Nom de la personne
    pub nom: String,

    /// Prénom de la personne
    pub prenom: String,

    /// Département de résidence de la personne.
    pub departement_code: Option<String>,

    /// Adresse email du client
    pub email: Option<String>,

    /// Association réciproque de AvisClient.Client
    pub avis_clients: Vec<i32>,
}
