using System;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
namespace GoogleMapNET
{
    class Program
    {
        enum Place
        {
            accounting,
            //airport,
            amusement_park,
            //aquarium,
            //art_gallery,
            atm,
            //bakery,
            bank,
            //bar,
            //beauty_salon,
            //bicycle_store,
            book_store,
            //bowling_alley,
            bus_station,
            //cafe,
            campground,
            //car_dealer,
            //car_rental,
            car_repair,
            //car_wash,
            //casino,
            //cemetery,
            //church,
            //city_hall,
            clothing_store,
            convenience_store,
            //courthouse,
            //dentist,
            department_store,
            doctor,
            //electrician,
            //electronics_store,
            //embassy,
            fire_station,
            //florist,
            //funeral_home,
            furniture_store,
            //gas_station,
            //gym,
            //hair_care,
            hardware_store,
            //hindu_temple,
            home_goods_store,
            hospital,
            insurance_agency,
            //jewelry_store,
            //laundry,
            //lawyer,
            library,
            //liquor_store,
            local_government_office,
            //locksmith,
            //lodging,
            //meal_delivery,
            //meal_takeaway,
            //mosque,
            //movie_rental,
            //movie_theater,
            //moving_company,
            museum,
            //night_club,
            //painter,
            //park,
            //parking,
            //pet_store,
            //pharmacy,
            //physiotherapist,
            //plumber,
            police,
            //post_office,
            //real_estate_agency,
            restaurant,
            //roofing_contractor,
            //rv_park,
            school,
            shoe_store,
            shopping_mall,
            //spa,
            stadium,
            //storage,
            store,
            //subway_station,
            //synagogue,
            //taxi_stand,
            train_station,
            //transit_station,
            //travel_agency,
            university,
            //veterinary_care,
            zoo
        };

        static void Main(string[] args)
        {
            StringBuilder strBuilder = new StringBuilder();

            int RADIUS = 1000; //Radius 1KM
            string googleMapKey = getMyApi();
            string url = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&radius={2}&type={3}&key={4}";
            string path = @"json\{0}_{1}.json";
            string connString = "Server=.;Database=TEST;Trusted_Connection=True;";

            string[] places = Enum.GetNames(typeof(Place));


            using (SqlConnection connInsert = new SqlConnection(connString))
            {
                connInsert.Open();
                using (SqlCommand commInsert = new SqlCommand("TRUNCATE TABLE OFFICE_PLACE", connInsert))
                {
                    commInsert.ExecuteNonQuery();
                }

            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "SELECT a.OFFICE_CODE,b.DISPLAY_NAME,a.LATITUDE,a.LONGITUDE FROM OFFICE_GEOTAG a";
                query += " INNER JOIN OFFICE_NOC b ON a.OFFICE_CODE = b.OFFICE_CODE";
                query += " WHERE a.OFFICE_CODE IN ('W0090','W0183','W0537','W0710','W0766','W0940','W0944','W0979','W0999','W1222','W1196'";
                query += ",'W1751','W1752','W1759','W1768','W1773','W1811','W1910','W1966','W1442')";

                using (SqlCommand comm = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = comm.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            //OFFICE_CODE~PLACE_TYPE~PLACE_NAME~LATITUDE~LONGITUDE
                            strBuilder.AppendFormat("{0}~{1}~{2}~{3}~{4}\r\n",reader[0].ToString(),"1.Wisma",reader[1].ToString(),reader[2].ToString(),reader[3].ToString());

                            foreach (string place in places)
                            {
                                string urls = String.Format(url, reader[2].ToString(), reader[3].ToString(), RADIUS, place, googleMapKey);
                                Console.WriteLine("Process {0}", urls);
                                //Console.ReadLine();
                                var request = (HttpWebRequest)WebRequest.Create(urls);

                                try
                                {
                                    var response = (HttpWebResponse)request.GetResponse();
                                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                    using (FileStream fs = File.Create(String.Format(path, reader[0].ToString(), place.ToString())))
                                    {
                                        Byte[] content = new UTF8Encoding(true).GetBytes(responseString);

                                        fs.Write(content, 0, content.Length);

                                        Model.JsonObject jsons = JsonConvert.DeserializeObject<Model.JsonObject>(responseString);

                                        if (!jsons.status.Equals("ZERO_RESULTS"))
                                        {
                                            foreach (Model.Result rst in jsons.results)
                                            {
                                                query = String.Format("insert into OFFICE_PLACE VALUES('{0}','{1}','{2}','{3}',{4},{5})", reader[0].ToString(), place.ToString(), rst.name, rst.icon, rst.geometry.location.lat, rst.geometry.location.lng);
                                                using (SqlConnection connInsert = new SqlConnection(connString))
                                                {
                                                    connInsert.Open();
                                                    using (SqlCommand commInsert = new SqlCommand(query, connInsert))
                                                    {
                                                        commInsert.ExecuteNonQuery();
                                                    }
                                                }
                                                strBuilder.AppendFormat("{0}~{1}~{2}~{3}~{4}\r\n", reader[0].ToString(), place.ToString(), rst.name, rst.geometry.location.lat, rst.geometry.location.lng,1);
                                            }//end foreach

                                        }
                                        else
                                        {
                                            Console.WriteLine(jsons.status.ToString());
                                        }//end if
                                       
                                        Console.WriteLine(String.Format("{0}_{1} --> done", reader[0].ToString(), place.ToString()));
                                    }

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }//end foreach

                        }//end while
                        using (StreamWriter file = new StreamWriter("office_geotag_file"))
                        {
                            file.WriteLine(strBuilder.ToString());
                        }
                    }
                }
            }// end using
            strBuilder = null;

            Console.WriteLine("Done All");
            Console.ReadLine();
        }

        private static string getMyApi()
        {
            string oldApi = "AIzaSyDrEzbUl9lQyEX8fxyireBP1bipoV51ydQ";
            string newApi = "AIzaSyBtK52SuWG7MNeeq6q36OSSpnnXcqWQaUQ";

            return newApi;
        }
    }//end class
}//end namespace
