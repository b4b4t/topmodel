//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};
use crate::restaurant::avis_client::AvisClient;

/// Client du restaurant
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct Client {
    /// Adresse email du client
    pub email: Option<String>,

    /// Association réciproque de AvisClient.Client
    pub avis_clients: Vec<AvisClient>,
}
