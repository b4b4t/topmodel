//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};
use crate::restaurant::avis_client::AvisClient;

/// Client du restaurant
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Client {
    /// Adresse email du client
    pub email: Option<String>,

    /// Association réciproque de AvisClient.Client
    pub avis_clients: Vec<AvisClient>,
}
