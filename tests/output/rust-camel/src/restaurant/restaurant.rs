//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};
use crate::restaurant::avis_client::AvisClient;
use crate::restaurant::menu::Menu;
use crate::restaurant::plat::Plat;
use crate::restaurant::promotion::Promotion;

/// Restaurant
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct Restaurant {
    /// Identifiant du restaurant
    pub id: i32,

    /// Nom du restaurant
    pub nom: String,

    /// Adresse du restaurant
    pub adresse: Option<String>,

    /// Numéro de téléphone
    pub telephone: Option<String>,

    /// Association réciproque de Menu.Restaurant
    pub menus: Vec<Menu>,

    /// Association réciproque de Plat.Restaurant
    pub plats: Vec<Plat>,

    /// Association réciproque de Promotion.Restaurant
    pub promotions: Option<Vec<Promotion>>,

    /// Association réciproque de AvisClient.Restaurant
    pub avis_clients: Vec<AvisClient>,

    /// Association réciproque de TableRestaurant.RestaurantId
    pub table_ids: Vec<i32>,
}
