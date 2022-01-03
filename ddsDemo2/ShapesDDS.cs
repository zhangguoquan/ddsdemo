using DDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ddsDemo2
{
    class ShapesDDS
    {
        private DomainParticipant participant = null;
        private Publisher publisher = null;
        private Subscriber subscriber = null;

        private Topic topic_shape = null;

        public Boolean initialize(int domainID)
        {

            participant =
                   DDS.DomainParticipantFactory.get_instance().create_participant(
                       domainID,
                       DDS.DomainParticipantFactory.PARTICIPANT_QOS_DEFAULT,
                       null /* listener */,
                       DDS.StatusMask.STATUS_MASK_NONE);
            if (participant == null)
            {
                shutdown(participant);
                throw new ApplicationException("create_participant error");
            }


            /* Register the type before creating the topic */
            System.String type_name = ShapeTypeTypeSupport.get_type_name();
            try
            {
                ShapeTypeTypeSupport.register_type(
                    participant, type_name);
            }
            catch (DDS.Exception e)
            {
                Console.WriteLine("register_type error {0}", e);
                shutdown(participant);
                throw e;
            }


            publisher = participant.create_publisher(
                DDS.DomainParticipant.PUBLISHER_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
            if (publisher == null)
            {
                shutdown(participant);
                throw new ApplicationException("create_publisher error");
            }



            subscriber = participant.create_subscriber(
                DDS.DomainParticipant.SUBSCRIBER_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
            if (subscriber == null)
            {
                shutdown(participant);
                throw new ApplicationException("create_subscriber error");
            }

            topic_shape = participant.create_topic(
                      "Shape",
                      type_name,
                      DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
                      null /* listener */,
                      DDS.StatusMask.STATUS_MASK_NONE);
            if (topic_shape == null)
            {
                shutdown(participant);
                throw new ApplicationException("create_topic error");
            }
            return true;
        }

        public void stop()
        {
            participant.delete_contained_entities();
            DomainParticipantFactory.get_instance().delete_participant(ref participant);
        }

        static void shutdown(
            DDS.DomainParticipant participant)
        {

            /* Delete all entities */

            if (participant != null)
            {
                participant.delete_contained_entities();
                DDS.DomainParticipantFactory.get_instance().delete_participant(
                    ref participant);
            }

        }


        public ShapeTypeDataWriter create_writer(Topic topic)
        {
            return (ShapeTypeDataWriter)publisher.create_datawriter(
                     topic, Publisher.DATAWRITER_QOS_DEFAULT,
                     null, StatusMask.STATUS_MASK_NONE);
        }

        public DataReader create_reader(Topic topic, DataReaderListener listener)
        {
            return subscriber.create_datareader(topic, Subscriber.DATAREADER_QOS_DEFAULT, listener, StatusMask.STATUS_MASK_ALL);
        }
        public Topic get_Topic()
        {
            return topic_shape;
        }

       
    }
   


}
