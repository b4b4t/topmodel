//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::statut_commande::StatutCommande;

/// Client avec la liste de ses commandes
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ClientAvecCommandes {
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

    /// Liste des commandes du client
    pub commande_id: Vec<i32>,

    /// Liste des commandes du client
    pub commande_date_commande: Vec<NaiveDateTime>,

    /// Liste des commandes du client
    pub commande_date_livraison: Option<Vec<NaiveDateTime>>,

    /// Liste des commandes du client
    pub commande_montant_total: Vec<Decimal>,

    /// Liste des commandes du client
    pub commande_client_id: Vec<i32>,

    /// Liste des commandes du client
    pub commande_table_id: Option<Vec<i32>>,

    /// Liste des commandes du client
    pub commande_reservation_id: Option<Vec<i32>>,

    /// Liste des commandes du client
    pub commande_statut_commande: Vec<StatutCommande>,

    /// Liste des commandes du client
    pub commande_avis_client_id: Option<Vec<i32>>,

    /// Liste des commandes du client
    pub commande_lignes: Vec<Vec<i32>>,
}
