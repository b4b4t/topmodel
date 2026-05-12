//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// Restaurant avec ses statistiques
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RestaurantAvecStatistiques {
    /// Identifiant du restaurant
    pub id: i32,

    /// Nom du restaurant
    pub nom: String,

    /// Adresse du restaurant
    pub adresse: Option<String>,

    /// Numéro de téléphone
    pub telephone: Option<String>,

    /// Association réciproque de Menu.Restaurant
    pub menus: Vec<i32>,

    /// Association réciproque de Plat.Restaurant
    pub plats: Vec<i32>,

    /// Association réciproque de Promotion.Restaurant
    pub promotions: Option<Vec<i32>>,

    /// Association réciproque de AvisClient.Restaurant
    pub avis_clients: Vec<i32>,

    /// Association réciproque de TableRestaurant.RestaurantId
    pub table_ids: Vec<i32>,

    /// Nombre de plats du restaurant
    pub nombre_plats: i32,

    /// Nombre de tables du restaurant
    pub nombre_tables: i32,

    /// Note moyenne des avis clients
    pub note_moyenne: Option<Decimal>,
}
